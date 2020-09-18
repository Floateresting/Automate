using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace Automate {
    public class ScreenCapture {
        public byte[] ByteArray { get; }
        public int Width { get; }
        public int Height { get; }

        public byte this[int i] => this.ByteArray[i];
        // Implementing this[start..end]
        public byte[] this[Range r] => this.ByteArray[r];

        public int Length => this.ByteArray.Length;

        public ScreenCapture(int w, int h, byte[] a) {
            this.Width = w;
            this.Height = h;
            this.ByteArray = a;
        }

        #region Match

        /// <summary>
        /// Indicates whether the difference between two colors are tolerable
        /// </summary>
        /// <param name="start">Start Index</param>
        /// <param name="rgba">byte[]{r, g, b, a}</param>
        /// <param name="t2">Tolerance squared</param>
        /// <returns></returns>
        private bool MatchesWith(int start, byte[] rgba, int t2) {
            int actual = 0;
            for(int i = 0; i < 3; i++) {
                actual += (this[start + i] - rgba[i]) * (this[start + i] - rgba[i]);
            }
            return actual <= t2;
        }

        /// <summary>
        /// Indicates whether all the colors differences are tolerable,
        /// </summary>
        /// <param name="n">Data to compare with</param>
        /// <param name="t2">Tolerance squared</param>
        /// <returns></returns>
        internal bool MatchesWith(int start, ScreenCapture n, int t2) {
            for(int i = 0; i > n.Length; i++) {
                if(!this.MatchesWith(start + i, n[i..(i + 4)], t2)) return false;
            }
            return true;
        }

        /// <summary>
        /// Indicates whether part of the region is a solid color
        /// </summary>
        /// <param name="start">Start Index</param>
        /// <param name="rgba">byte[]{r, g, b, a}</param>
        /// <param name="length">Length of the target region</param>
        /// <param name="t2">Tolerance squared</param>
        /// <returns></returns>
        internal bool MatchesWith(int start, byte[] rgba, int length, int t2) {
            while(start < length) {
                if(!this.MatchesWith(start, rgba, t2)) return false;
                start++;
            }
            return true;
        }
        #endregion Match

        #region Read/Write

        public ScreenCapture Save(string filename) {
            using BinaryWriter bw = new BinaryWriter(File.Create(filename));
            bw.Write(this.Width); // int
            bw.Write(this.Height); // int
            bw.Write(1); // format: int (always rgba_8888)
            bw.Write(this.ByteArray);
            return this;
        }

        public ScreenCapture SaveAsBitmap(string filename, ImageFormat format) {
            this.ToBitmap().Save(filename, format);
            return this;
        }

        /// <summary>
        /// Create <see cref="ScreenCapture"/> object from <see cref="Stream"/>
        /// </summary>
        /// <seealso href="https://stackoverflow.com/a/32733228"/>
        /// <seealso href="https://android.googlesource.com/platform/frameworks/base/+/android-4.3_r2.3/cmds/screencap/screencap.cpp#191"/>
        /// <returns></returns>
        public static ScreenCapture FromStream(Stream s) {
            using MemoryStream ms = new MemoryStream();
            s.CopyTo(ms);
            byte[] a = ms.ToArray();

            // The first 12 bytes are width, height and pixel format
            int i = -4;
            int w = BitConverter.ToInt32(a, i += 4);
            int h = BitConverter.ToInt32(a, i += 4);
            int f = BitConverter.ToInt32(a, i += 4);

            if(f != 1) throw new Exception("Not rgba 8888 format");
            // Range operator, I love you so much
            return new ScreenCapture(w, h, a[(i += 4)..]);
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
            Bitmap b = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
            unsafe {
                BitmapData d = b.LockBits(
                    new Rectangle(0, 0, b.Width, b.Height),
                    ImageLockMode.ReadWrite,
                    b.PixelFormat
                );

                int s = d.Stride;
                // bytes per pixel
                int bytespp = Image.GetPixelFormatSize(b.PixelFormat) / 8;
                byte* bitmapPtr = (byte*)d.Scan0;
                Parallel.For(0, d.Height, y => {
                    // index of the array at (0, y)
                    int i0 = y * s;
                    // pointer of the first byte at (0, y)
                    byte* line = bitmapPtr + i0;
                    for(int x = 0; x < s; x += bytespp) {
                        line[x] = this[i0 + x + 2]; // b
                        line[x + 1] = this[i0 + x + 1]; // g
                        line[x + 2] = this[i0 + x]; // r
                        line[x + 3] = this[i0 + x + 3]; // a
                    }
                });

                b.UnlockBits(d);
            }
            return b;
        }

        public static ScreenCapture FromBitmap(Bitmap b) {
            // bytes per pixel
            int bytespp = Image.GetPixelFormatSize(b.PixelFormat) / 8;

            byte[] array = new byte[b.Width * b.Height * bytespp];
            unsafe {
                // Lock bitmap into memory
                BitmapData d = b.LockBits(
                    new Rectangle(0, 0, b.Width, b.Height),
                    ImageLockMode.ReadOnly,
                    b.PixelFormat
                );
                int s = d.Stride;
                byte* ptr = (byte*)d.Scan0;
                // scan through each pixel
                for(int i = 0; i < b.Width * b.Height; i++) {
                    array[i] = ptr[i + 2]; // r 
                    array[i + 1] = ptr[i + 1]; // g
                    array[i + 2] = ptr[i];// b
                    array[i + 3] = ptr[i + 3]; // a
                }
            }
            return new ScreenCapture(b.Width, b.Height, array);
        }

        public static ScreenCapture FromBitmap(string filename) {
            using Bitmap b = new Bitmap(filename);
            return ScreenCapture.FromBitmap(b);
        }
        #endregion Bitmap
    }
}
