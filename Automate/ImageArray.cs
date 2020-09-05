using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Automate {
    public class ImageArray {
        public byte[,][] Data { get; }
        public int Width => this.Data.GetLength(0);
        public int Height => this.Data.GetLength(1);

        public byte[] this[int x, int y] {
            get => this.Data[x, y];
            set => this.Data[x, y] = value;
        }

        public ImageArray(int w, int h) {
            this.Data = new byte[w, h][];
        }

        #region Match

        /// <summary>
        /// Indicates whether the difference between two colors are tolerable
        /// </summary>
        /// <param name="rgba">{r, g, b, a}</param>
        /// <param name="t">tolerance squared</param>
        /// <returns></returns>
        private bool MatchesWith(int x, int y, byte[] rgba, int t) {
            int actual = 0;
            // Transparent pixels: not implemented
            for(int i = 0; i < 3; i++) {
                actual += (this[x, y][i] - rgba[i]) * (this[x, y][i] - rgba[i]);
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
        internal bool MatchesWith(int x1, int y1, ImageArray needle, int t) {
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
        /// <param name="x1">Start x position</param>
        /// <param name="y1">Start y position</param>
        /// <param name="rgba">{r, g, b, a}</param>
        /// <param name="w">Width of the solid color region</param>
        /// <param name="h">Height of the solid color region</param>
        /// <param name="t">Tolerance squared</param>
        /// <returns></returns>
        internal bool MatchesWith(int x1, int y1, byte[] rgba, int w, int h, int t) {
            for(int x2 = 0; x2 < w; x2++) {
                for(int y2 = 0; y2 < h; y2++) {
                    if(!this.MatchesWith(x1 + x2, y1 + y2, rgba, t)) {
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion Match

        #region Read/Write

        public void Save(string filename) {
            using BinaryWriter bw = new BinaryWriter(File.Create(filename));
            bw.Write(this.Width); // int
            bw.Write(this.Height); // int
            bw.Write(1); // format: int (always rgba_8888)
            for(int y = 0; y < this.Height; y++) {
                for(int x = 0; x < this.Width; x++) {
                    bw.Write(this[x, y]);
                }
            }
        }

        /// <summary>
        /// Create <see cref="ImageArray"/> object from <see cref="Stream"/>
        /// </summary>
        /// <seealso href="https://stackoverflow.com/a/32733228"/>
        /// <seealso href="https://android.googlesource.com/platform/frameworks/base/+/android-4.3_r2.3/cmds/screencap/screencap.cpp#191"/>
        /// <returns></returns>
        public static ImageArray FromStream(Stream s) {
            using BinaryReader br = new BinaryReader(s);
            // width, height, format
            int w = br.ReadInt32();
            int h = br.ReadInt32();
            int f = br.ReadInt32();

            if(f != 1) throw new Exception("Not rgba 8888 format");

            ImageArray sc = new ImageArray(w, h);
            for(int y = 0; y < h; y++) {
                for(int x = 0; x < w; x++) {
                    // r, g, b, a
                    sc[x, y] = br.ReadBytes(4);
                }
            }
            return sc;
        }

        public static ImageArray FromFile(string filename) {
            return ImageArray.FromStream(File.OpenRead(filename));
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

        public static ImageArray FromBitmap(Bitmap b) {
            ImageArray sc = new ImageArray(b.Width, b.Height);
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

        public static ImageArray FromBitmap(string filename) {
            using Bitmap b = new Bitmap(filename);
            return ImageArray.FromBitmap(b);
        }
        #endregion Bitmap
    }
}
