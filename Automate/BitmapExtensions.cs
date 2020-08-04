using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Automate {
    public static class BitmapExtensions {
        private static bool MatchPixel(byte[] rgb1, byte[] rgb2, int t) {
            int actual = 0;
            for(int i = 0; i < 3; i++) {
                actual += (rgb1[i] - rgb2[i]) * (rgb1[i] - rgb2[i]);
            }
            return actual <= t;
        }

        private static bool MatchRegion(this byte[,][] a1, int x1, int y1, byte[,][] a2, int t) {
            for(int x2 = 0; x2 < a2.GetLength(0); x2++) {
                for(int y2 = 0; y2 < a2.GetLength(1); y2++) {
                    if(!MatchPixel(a1[x1 + x2, y1 + y2], a2[x2, y2], t)) {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Convert the bitmap to array
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>byte[x,y][]</returns>
        private static byte[,][] ToArray(this Bitmap bitmap) {
            byte[,][] array = new byte[bitmap.Width, bitmap.Height][];

            unsafe {
                // Lock bitmap into memory
                BitmapData data = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    bitmap.PixelFormat
                );
                int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                int heightInPixels = data.Height;
                int widthInBytes = bitmap.Width * bytesPerPixel;
                byte* firstPixel = (byte*)data.Scan0;

                // scan through each row
                for(int y = 0; y < bitmap.Height; y++) {
                    byte* currentLine = firstPixel + y * data.Stride;
                    for(int x = 0; x < widthInBytes; x += bytesPerPixel) {
                        array[x / bytesPerPixel, y] = new byte[] {
                            currentLine[x + 2], // r
                            currentLine[x + 1], // g
                            currentLine[x], // b
                        };
                    }
                }
                bitmap.UnlockBits(data);
            }
            return array;
        }

        /// <summary>
        /// Seach for a bitmap image
        /// </summary>
        /// <param name="heystack"></param>
        /// <param name="needle">The template/smaller image</param>
        /// <param name="tolerance">Maximum difference between colors</param>
        /// <param name="result"></param>
        /// <returns>true if the needle is found</returns>
        public static bool Seach(this Bitmap heystack, Bitmap needle, int tolerance, out Point result) {
            // tolerance squared
            tolerance *= tolerance;

            byte[,][] harr = heystack.ToArray(), narr = needle.ToArray();
            // heystack.Height - needle.Height, so the needle won't be outside of heystack (same for width)
            for(int hy = 0; hy < heystack.Height - needle.Height; hy++) {
                for(int hx = 0; hx < heystack.Width - needle.Width; hx++) {
                    if(MatchRegion(harr, hx, hy, narr, tolerance)) {
                        // Get middle point
                        result = new Point(hx + needle.Width / 2, hy + needle.Height / 2);
                        return true;
                    }
                }
            }
            result = new Point();
            return false;
        }
    }
}
