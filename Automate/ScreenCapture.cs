using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Automate {
    public class ScreenCapture {
        public byte[,][] Data { get; }
        public int Width => this.Data.GetLength(0);
        public int Height => this.Data.GetLength(1);

        public byte[] this[int x, int y] {
            get => this.Data[x, y];
            set => this.Data[x, y] = value;
        }

        public ScreenCapture(int w, int h) {
            this.Data = new byte[w, h][];
        }

        #region Match

        /// <summary>
        /// Indicates whether the difference between two colors are tolerable
        /// </summary>
        /// <param name="t">tolerance squared</param>
        /// <returns></returns>
        private bool MatchesWith(int x, int y, byte[] pixel, int t) {
            int actual = 0;
            // Transparent pixels: not implemented
            for(int i = 0; i < 3; i++) {
                actual += (this[x, y][i] - pixel[i]) * (this[x, y][i] - pixel[i]);
            }
            return actual <= t;
        }

        /// <summary>
        /// Indicates whether all the colors differences are tolerable,
        /// </summary>
        /// <param name="x1">Start x position</param>
        /// <param name="y1">Start y position</param>
        /// <param name="needle">Data to compare with</param>
        /// <param name="t">Tolerance squared</param>
        /// <returns></returns>
        private bool MatchesWith(int x1, int y1, ScreenCapture needle, int t) {
            for(int x2 = 0; x2 < needle.Width; x2++) {
                for(int y2 = 0; y2 < needle.Height; y2++) {
                    if(!this.MatchesWith(x1 + x2, y1 + y2, needle[x2, y2], t)) {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Indicates whether part of the region is a solid color
        /// </summary>
        /// <param name="x">Start x position</param>
        /// <param name="y">Start y position</param>
        /// <param name="color">{r, g, b}</param>
        /// <param name="size">Size of the solid color region</param>
        /// <param name="t">Tolerance squared</param>
        /// <returns></returns>
        private bool MatchesWith(int x1, int y1, byte[] color, Size size, int t) {
            for(int x2 = 0; x2 < size.Width; x2++) {
                for(int y2 = 0; y2 < size.Height; y2++) {
                    if(!this.MatchesWith(x1 + x2, y1 + y2, color, t)) {
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion Match

        #region Locate

        /// <summary>
        /// Compare 2 <see cref="ScreenCapture"/>
        /// </summary>
        /// <param name="needle"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public Point LocateRaw(ScreenCapture needle, int t) {
            // tolerance squared
            t *= t;
            // h.GL(1) - n.GL(1) so the needle won't be outside of heystack ( same for GL(0) )
            for(int y = 0; y < this.Height - needle.Height; y++) {
                for(int x = 0; x < this.Width - needle.Width; x++) {
                    if(this.MatchesWith(x, y, needle, t)) {
                        // return middle point
                        return new Point(x + needle.Width / 2, y + needle.Height / 2);
                    }
                }
            }
            return Point.Empty;
        }
        #endregion Locate

        #region Read/Write

        public void SaveAs(string filename) {
            using BinaryWriter bw = new BinaryWriter(File.Create(filename));
            bw.Write(this.Width); // int
            bw.Write(this.Height); // int
            bw.Write(1); // format: int (always rgba_8888)
            for(int x = 0; x < this.Width; x++) {
                for(int y = 0; y < this.Height; y++) {
                    bw.Write(this[x, y]);
                }
            }
        }

        /// <summary>
        /// Create <see cref="ScreenCapture"/> object from <see cref="Stream"/>
        /// </summary>
        /// <seealso href="https://stackoverflow.com/a/32733228"/>
        /// <seealso href="https://android.googlesource.com/platform/frameworks/base/+/android-4.3_r2.3/cmds/screencap/screencap.cpp#191"/>
        /// <returns></returns>
        public static ScreenCapture FromStream(Stream s) {
            using BinaryReader br = new BinaryReader(s);
            // width, height, format
            int w = br.ReadInt32();
            int h = br.ReadInt32();
            int f = br.ReadInt32();

            if(f != 1) throw new Exception("Not rgba 8888 format");

            ScreenCapture sc = new ScreenCapture(w, h);
            for(int y = 0; y < h; y++) {
                for(int x = 0; x < h; x++) {
                    sc[x, y] = new byte[] {
                        br.ReadByte(), // r
                        br.ReadByte(), // g
                        br.ReadByte(), // b
                        br.ReadByte(), // a
                    };
                }
            }
            return sc;
        }

        public static ScreenCapture FromFile(string filename) {
            return ScreenCapture.FromStream(File.OpenRead(filename));
        }
        #endregion Read/Write

        #region Bitmap Conversion

        /// <summary>
        /// Convert byte array to a <see cref="Bitmap"/> by not using <see cref="Bitmap.SetPixel(int, int, Color)"/>
        /// </summary>
        /// <returns></returns>
        public Bitmap ToBitmap() {
            Bitmap b = new Bitmap(this.Width, this.Height);
            BitmapData d = b.LockBits(
                new Rectangle(0, 0, this.Width, this.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb
            );
            int s = d.Stride;

            unsafe {
                byte* ptr = (byte*)d.Scan0;
                for(int x = 0; x < this.Width; x++) {
                    for(int y = 0; y < this.Height; y++) {
                        ptr[x * 4 + y * s] = this[x, y][2]; // b
                        ptr[x * 4 + y * s + 1] = this[x, y][1]; // g
                        ptr[x * 4 + y * s + 2] = this[x, y][0]; // r
                        ptr[x * 4 + y * s + 3] = this[x, y][3]; // a
                    }
                }
            }
            b.UnlockBits(d);
            return b;
        }

        public static ScreenCapture FromBitmap(Bitmap b) {
            ScreenCapture sc = new ScreenCapture(b.Width, b.Height);
            // Lock bitmap into memory
            BitmapData d = b.LockBits(
                new Rectangle(0, 0, b.Width, b.Height),
                ImageLockMode.ReadOnly,
                b.PixelFormat
            );
            int s = d.Stride;
            int bytesPerPixel = Bitmap.GetPixelFormatSize(b.PixelFormat) / 8;
            unsafe {
                byte* ptr = (byte*)d.Scan0;
                // scan through each row
                for(int y = 0; y < b.Height; y++) {
                    byte* line = ptr + y * s;
                    for(int x = 0, i = 0; x < b.Width; x++, i += bytesPerPixel) {
                        sc[x, y] = new byte[] {
                            line[i+2], // r
                            line[i+1], // g
                            line[i], // b
                            line[i+3] // a
                        };
                    }
                }
            }
            return sc;
        }
        #endregion Bitmap
    }
}
