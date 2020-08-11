using System.Drawing;

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
    }
}
