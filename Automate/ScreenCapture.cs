using System;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Automate {
    public class ScreenCapture {
        public const int BytesPerPixel = 4;
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
        private string ToHex(byte[] rgb) {
            return (rgb[0] * 0x1000 + rgb[1] * 0x100 + rgb[2]).ToString("x");
        }
        private (int, int) ToCoord(int i) {
            return (i % this.Width, i / this.Width);
        }

        /// <summary>
        /// Check if every pixel on a line matches
        /// </summary>
        /// <param name="from">Start index inclusive</param>
        /// <param name="to">End index exclusive</param>
        /// <param name="rgb">Color in {r, g, b}</param>
        /// <param name="t2">Tolerance squared</param>
        /// <returns></returns>
        private bool MatchesLine(int from, int to, byte[] rgb, int t2) {
            int sum = 0;

            for(int i = from; i < to; i++) {
                for(int b = 0; b < 3; b++) {
                    sum += (this[i * 4 + b] - rgb[b]) * (this[i * 4 + b] - rgb[b]);
                }
            }
            return sum / (to - from) <= t2;
        }

        /// <summary>
        /// Check if every pixel in a region matches
        /// </summary>
        /// <param name="start">Start pixel index</param>
        /// <param name="width">Width in number of pixels</param>
        /// <param name="height">Height in number of pixels</param>
        /// <param name="rgb">{r, g, b}</param>
        /// <param name="t2">Tolerance squared</param>
        /// <returns></returns>
        internal bool MatchesRectangle(int start, int width, int height, byte[] rgb, int t2) {
            for(int y = 0; y < height; y++) {
                int from = start + y * this.Width;
                int to = from + width;
                if(!this.MatchesLine(from, to, rgb, t2)) return false;
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
        public unsafe Bitmap ToBitmap() {
            Bitmap b = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
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
            return b;
        }

        public static unsafe ScreenCapture FromBitmap(Bitmap b) {
            // bytes per pixel
            int bytespp = Image.GetPixelFormatSize(b.PixelFormat) / 8;

            byte[] array = new byte[b.Width * b.Height * bytespp];
            // Lock bitmap into memory
            BitmapData d = b.LockBits(
                new Rectangle(0, 0, b.Width, b.Height),
                ImageLockMode.ReadOnly,
                b.PixelFormat
            );
            int s = d.Stride;
            byte* ptr = (byte*)d.Scan0;
            // Scan though each pixel
            Parallel.For(0, d.Width * d.Height, i => {
                i *= bytespp;
                array[i] = ptr[i + 2]; // r
                array[i + 1] = ptr[i + 1];// g
                array[i + 2] = ptr[i]; // b
                array[i + 3] = ptr[i + 3];// a
            });
            return new ScreenCapture(b.Width, b.Height, array);
        }

        public static ScreenCapture FromBitmap(string filename) {
            Bitmap b = new Bitmap(filename);
            return ScreenCapture.FromBitmap(b);
        }
        #endregion Bitmap
    }
}
