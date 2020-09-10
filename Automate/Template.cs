namespace Automate {
    public struct Template {
        public byte[] Color { get; }
        public int Size { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Tolerance2 { get; }

        /// <summary>
        /// Initialize a template
        /// </summary>
        /// <param name="hex">Target color in hex, e.g. 0x39c5bb</param>
        /// <param name="size">Minimum size of the color (will be searching for a square)</param>
        /// <param name="region">Expected region (for faster template matching)</param>
        /// <param name="tolerance">Maximum distance with the color</param>
        public Template(int hex, int size, (int X, int Y, int Width, int Height) region, int tolerance) {
            this.Color = Template.ToRGB(hex);
            this.Size = size;
            this.X = region.X;
            this.Y = region.Y;
            this.Width = region.Width;
            this.Height = region.Height;
            this.Tolerance2 = tolerance * tolerance;
        }

        /// <summary>
        /// Convert a hex color to <see cref="byte[]"/>
        /// e.g. 0x39c5bb to { 0x39, 0xc5, 0xbb }
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] ToRGB(int hex) {
            byte[] rgb = new byte[3];
            for(int i = rgb.Length; i-- > 0; hex >>= 8) {
                rgb[i] = (byte)(hex & 0xff);
            }
            return rgb;
        }
    }
}
