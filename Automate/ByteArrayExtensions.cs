namespace Automate {
    public static class ByteArrayExtensions {
        private static bool MatchPixel(byte[] rgb1, byte[] rgb2, int t) {
            int actual = 0;
            for(int i = 0; i < 3; i++) {
                actual += (rgb1[i] - rgb2[i]) * (rgb1[i] - rgb2[i]);
            }
            return actual <= t;
        }

        public static bool MatchRegion(this byte[,][] a1, int x1, int y1, byte[,][] a2, int t) {
            for(int x2 = 0; x2 < a2.GetLength(0); x2++) {
                for(int y2 = 0; y2 < a2.GetLength(1); y2++) {
                    if(!MatchPixel(a1[x1 + x2, y1 + y2], a2[x2, y2], t)) {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
