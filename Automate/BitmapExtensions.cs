using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Automate {
    public static class BitmapExtensions {
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
        /// Search for a bitmap image, return the 1st result
        /// </summary>
        /// <param name="heystack"></param>
        /// <param name="needle">The template/smaller image</param>
        /// <param name="tolerance">Maximum difference between colors</param>
        /// <returns></returns>
        public static Point LocateBitmap(this Bitmap heystack, Bitmap needle, int tolerance) {
            // tolerance squared
            tolerance *= tolerance;

            byte[,][] harr = heystack.ToArray(), narr = needle.ToArray();
            // heystack.Height - needle.Height, so the needle won't be outside of heystack (same for width)
            for(int hy = 0; hy < heystack.Height - needle.Height; hy++) {
                for(int hx = 0; hx < heystack.Width - needle.Width; hx++) {
                    if(harr.MatchesWith(hx, hy, narr, tolerance)) {
                        // return middle point
                        return new Point(hx + needle.Width / 2, hy + needle.Height / 2);
                    }
                }
            }
            return Point.Empty;
        }

        /// <summary>
        /// Search for a bitmap image, return the all results
        /// </summary>
        /// <param name="heystack"></param>
        /// <param name="needle">The template/smaller image</param>
        /// <param name="tolerance">Maximum difference between colors</param>
        /// <param name="distance">Minimum distance between 2 found regions</param>
        /// <returns></returns>
        public static IEnumerable<Point> LocateBitmapAll(this Bitmap heystack, Bitmap needle, int tolerance, int distance) {
            tolerance *= tolerance;
            List<Rectangle> covered = new List<Rectangle>();

            byte[,][] harr = heystack.ToArray(), narr = needle.ToArray();
            for(int hy = 0; hy < heystack.Height - needle.Height; hy++) {
                for(int hx = 0; hx < heystack.Width - needle.Width; hx++) {
                    // Skip pixels that are in found results
                    if(covered.Select(r => r.Contains(hx, hy)).Any()) continue;
                    // Add rect and retun point if matches
                    if(harr.MatchesWith(hx, hy, narr, tolerance)) {
                        // The needle but with distance
                        covered.Add(new Rectangle(hx - distance, hy - distance, needle.Width + distance, needle.Height + distance));

                        yield return new Point(hx + needle.Width / 2, hy + needle.Height / 2);
                        // continue outside the needle
                        hx += needle.Width;
                    }
                }
            }
        }

        /// <summary>
        /// Search for a solid color
        /// </summary>
        /// <param name="heystack"></param>
        /// <param name="color"></param>
        /// <param name="size"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static Point LocateColor(this Bitmap heystack, byte[] color, Size size, int tolerance) {
            tolerance *= tolerance;
            byte[,][] harr = heystack.ToArray();
            for(int hy = 0; hy < heystack.Height - size.Height; hy++) {
                for(int hx = 0; hx < heystack.Width - size.Width; hx++) {
                    if(harr.MatchesWith(hx, hy, color, size, tolerance)) {
                        return new Point(hx + size.Width / 2, hy + size.Height / 2);
                    }
                }
            }
            return Point.Empty;
        }

        /// <summary>
        /// Search for a solid color, return the all results
        /// </summary>
        /// <param name="heystack"></param>
        /// <param name="color"></param>
        /// <param name="size"></param>
        /// <param name="tolerance"></param>
        /// <param name="distance">Minimum distance between 2 found regions</param>
        /// <returns></returns>
        public static IEnumerable<Point> LocateColorAll(this Bitmap heystack, byte[] color, Size size, int tolerance, int distance) {
            tolerance *= tolerance;
            List<Rectangle> covered = new List<Rectangle>();

            byte[,][] harr = heystack.ToArray();
            for(int hy = 0; hy < heystack.Height - size.Height; hy++) {
                for(int hx = 0; hx < heystack.Width - size.Width; hx++) {
                    if(covered.Select(c => c.Contains(hx, hy)).Any()) continue;
                    if(harr.MatchesWith(hx, hy, color, size, tolerance)) {
                        covered.Add(new Rectangle(hx - distance, hy - distance, size.Width + distance, size.Height + distance));
                        yield return new Point(hx + size.Width / 2, hy + size.Height / 2);
                        hx += size.Width;
                    }
                }
            }
        }
    }
}
