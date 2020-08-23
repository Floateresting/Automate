using System.Drawing;
using System.Drawing.Imaging;

namespace Automate {
    public static class ByteArrayExtensions {
        /// <summary>
        /// Indicates whether the difference between two colors are tolerable
        /// </summary>
        /// <param name="pixel1"></param>
        /// <param name="pixel2"></param>
        /// <param name="t">tolerance squared</param>
        /// <returns></returns>
        internal static bool MatchesWith(this byte[] pixel1, byte[] pixel2, int t) {
            int actual = 0;
            // Transparent pixels: not implemented
            for(int i = 0; i < 3; i++) {
                actual += (pixel1[i] - pixel2[i]) * (pixel1[i] - pixel2[i]);
            }
            return actual <= t;
        }

        /// <summary>
        /// Indicates whether all the colors differences are tolerable,
        /// </summary>
        /// <param name="region1">the bigger region</param>
        /// <param name="x1">start x position for region1</param>
        /// <param name="y1">start y position for region1</param>
        /// <param name="region2">the smaller region</param>
        /// <param name="t">tolerance squared</param>
        /// <returns></returns>
        internal static bool MatchesWith(this byte[,][] region1, int x1, int y1, byte[,][] region2, int t) {
            for(int x2 = 0; x2 < region2.GetLength(0); x2++) {
                for(int y2 = 0; y2 < region2.GetLength(1); y2++) {
                    if(!region1[x1 + x2, y1 + y2].MatchesWith(region2[x2, y2], t)) {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Indicates whether part of the region is a solid color
        /// </summary>
        /// <param name="region1">The bigger region</param>
        /// <param name="x1">Start x position for region1</param>
        /// <param name="y1">Start x position for region1</param>
        /// <param name="color"></param>
        /// <param name="size">Size of the solid color region</param>
        /// <param name="t">tolerance squared</param>
        /// <returns></returns>
        internal static bool MatchesWith(this byte[,][] region1, int x1, int y1, byte[] color, Size size, int t) {
            for(int x = 0; x < size.Width; x++) {
                for(int y = 0; y < size.Height; y++) {
                    if(!region1[x1 + x, y1 + y].MatchesWith(color, t)) {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Convert byte array to a <see cref="Bitmap"/> by not using <see cref="Bitmap.SetPixel(int, int, Color)"/>
        /// </summary>
        /// <param name="a">byte array received from screencap (adb)</param>
        /// <returns></returns>
        public static Bitmap ToBitmap(this byte[,][] a) {
            int w = a.GetLength(0), h = a.GetLength(1);
            Bitmap b = new Bitmap(w, h);
            Rectangle rect = new Rectangle(0, 0, w, h);
            BitmapData d = b.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int s = d.Stride;
            unsafe {
                byte* ptr = (byte*)d.Scan0;
                for(int x = 0; x < w; x++) {
                    for(int y = 0; y < h; y++) {
                        ptr[x * 4 + y * s] = a[x, y][2]; // b
                        ptr[x * 4 + y * s + 1] = a[x, y][1]; // g
                        ptr[x * 4 + y * s + 2] = a[x, y][0]; // r
                        ptr[x * 4 + y * s + 3] = a[x, y][3]; // a
                    }
                }
            }
            b.UnlockBits(d);
            return b;
        }
    }
}
