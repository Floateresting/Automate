namespace Automate {
    public static class ByteArrayExtensions {
        /// <summary>
        /// Indicates whether the difference between two colors are tolerable
        /// </summary>
        /// <param name="pixel1"></param>
        /// <param name="pixel2"></param>
        /// <param name="t">tolerance squared</param>
        /// <returns></returns>
        private static bool MatchesWith(this byte[] pixel1, byte[] pixel2, int t) {
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
        public static bool MatchesWidth(this byte[,][] region1, int x1, int y1, byte[,][] region2, int t) {
            for(int x2 = 0; x2 < region2.GetLength(0); x2++) {
                for(int y2 = 0; y2 < region2.GetLength(1); y2++) {
                    if(region1[x1 + x2, y1 + y2].MatchesWith(region2[x2, y2], t)) {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
