using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Automate {
    public static class BitmapExtensions {
        public static byte[][][] ToArray(this Bitmap bitmap) {
            // byte[y][x][r,g,b]
            byte[][][] array = new byte[bitmap.Height][][];
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
                for(int y = 0; y < heightInPixels; y++) {
                    array[y] = new byte[bitmap.Width][];
                    byte* currentLine = firstPixel + y * data.Stride;
                    for(int x = 0; x < widthInBytes; x += bytesPerPixel) {
                        array[y][x / bytesPerPixel] = new byte[] {
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
        public static bool Match(this Bitmap heystack, Bitmap needle,int tolerance,out Point result) {
            /// <summary>Check if all pixels match for 1 line</summary>
            /// <param name="hl">Line on the heystack</param>
            /// <param name="hs">Start position for hl</param>
            /// <param name="nl">Line on the needle</param>
            /// <param name="ns">Start position for nl</param>
            bool SeachFor1Line(byte[][] hl, int hs, byte[][] nl, int ns) {
                byte[] hRGB, nRGB;
                // actual 
                int actual;
                for(int x = 0; x < nl.Length - ns; x++) {
                    // Get rgb of the current pixel
                    hRGB = hl[x + hs];
                    nRGB = nl[x + ns];
                    // calculate the acutal difference^2 between the 2 colors
                    // (distance of 2 points in a 3D space)
                    actual = 0;
                    for(int i = 0; i < 3; i++) {
                        actual += (hRGB[i] - nRGB[i]) * (hRGB[i] - nRGB[i]);
                    }
                    if(actual > tolerance) {
                        return false;
                    }
                }
                return true;
            }

            byte[][][] harr = heystack.ToArray(), narr = needle.ToArray();
            // tolerance squared
            tolerance *= tolerance;
            // heystack.Height - needle.Height, so the needle won't be outside of heystack (same for width)
            for(int hy1 = 0; hy1 < heystack.Height - needle.Height; hy1++) {
                for(int hx = 0; hx < heystack.Width - needle.Width; hx++) {
                    // if the first line matches
                    if(SeachFor1Line(harr[hy1], hx, narr[0], 0)) {
                        for(int hy2 = hy1 + 1; hy2 < hy1 + narr.Length; hy2++) {
                            // if one of the other lines doesn't match
                            if(!SeachFor1Line(harr[hy2], hx, narr[hy2 - hy1], 0)) {
                                result = new Point();
                                return false;
                            }
                        }
                        result = new Point(hx + needle.Width / 2, hy1 + needle.Height / 2);
                        return true;
                    }
                }
            }
            result = new Point();
            return false;
        }
    }
}
