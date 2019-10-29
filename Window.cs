using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Automate {
    public class Window {
        // Issue with the "Bounds"
        //public double ratio = (double)Screen.PrimaryScreen.Bounds.Width / 1920;

        public int offsetX, offsetY, offsetW, offsetH;

        /// <summary>
        /// Default Contructor, params: section of the screen to work with
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public Window(int x, int y, int w, int h) {
            this.offsetX = x;
            this.offsetY = y;
            this.offsetW = w;
            this.offsetH = h;
        }

        /// <summary>
        /// Search for a .bmp image on the screen
        /// </summary>
        /// <param name="template">Bmp image to search</param>
        /// <param name="tolerance"></param>
        /// <returns>Center of the image on screen</returns>
        public (int, int) LocateOnScreen(string templatePath, int tolerance) {
            Bitmap template = new Bitmap(templatePath);

            // tolerance^2 = deltaR^2 + deltaG^2 + deltaB^2
            int toleranceSquared = tolerance * tolerance;

            byte[][][] GetBitmapArray(Bitmap bitmap) {
                // byte[y][x][r, g, b]
                byte[][][] bitmapArray = new byte[bitmap.Height][][];

                unsafe {
                    // Me from 2019-08-29: I don't understand either :(
                    // Me from 2019-10-24: Me neither :(
                    BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                    int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                    int heightInPixels = bitmapData.Height;
                    int widthInBytes = bitmapData.Width * bytesPerPixel;
                    byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                    // Search for each row (why I can't use Parallel.For)
                    for(int y = 0; y < heightInPixels; y++) {
                        bitmapArray[y] = new byte[bitmap.Width][];
                        byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                        for(int x = 0; x < widthInBytes; x += bytesPerPixel) {
                            bitmapArray[y][x / bytesPerPixel] = new byte[] {
                                    currentLine[x + 2],    //r
                                    currentLine[x + 1],    //g
                                    currentLine[x]         //b
                                };
                        }
                    }
                    bitmap.UnlockBits(bitmapData);
                }
                return bitmapArray;
            }

            // Check if all pixels match (only for 1 line)
            (int, bool) SearchFor1Line(byte[][] screenLine, int screenStart, byte[][] templateLine, int templateStart) {
                int maxDelta = 0;
                for(int x = 0; x < templateLine.Length; x++) {
                    // Get rgb of the corresp pixel
                    byte[] screenRGB = screenLine[x + screenStart];
                    byte[] templateRGB = templateLine[x + templateStart];
                    int delta = 0;
                    // Add 3 (rgb) squared deltas together
                    for(int i = 0; i < 3; i++) {
                        delta += (screenRGB[i] - templateRGB[i]) * (screenRGB[i] - templateRGB[i]);
                    }
                    if(delta > toleranceSquared) {
                        return (0, false);
                    }
                    maxDelta = Math.Max(maxDelta, delta);
                }
                return (maxDelta, true);
            }


            // Take screenshot
            Size screenshotSize = new Size(offsetW, offsetH);
            Bitmap screenshot = new Bitmap(offsetW, offsetH, PixelFormat.Format32bppRgb);
            Graphics graphics = Graphics.FromImage(screenshot);
            graphics.CopyFromScreen(offsetX, offsetY, 0, 0, screenshotSize, CopyPixelOperation.SourceCopy);


            // Get 2 images' array   byte[y][x][rgb]
            byte[][][] screenshotArray = GetBitmapArray(screenshot);
            byte[][][] templateArray = GetBitmapArray(template);

            // Help! foreach loop with index!!!!
            // Search for each line (screen)
            for(int y1 = 0; y1 < screenshotArray.Length - templateArray.Length; y1++) {
                // Search for each px on the line (screen)
                for(int x1 = 0; x1 < screenshotArray[y1].Length - templateArray[0].Length; x1++) {
                    // tolerance1: Tolerance on 1st line
                    //  match1: if the 1st line (screen) matches the 1st line (template)
                    (int tolerance1, bool match1) = SearchFor1Line(screenshotArray[y1], x1, templateArray[0], 0);
                    if(match1) {
                        bool match = true;
                        int tolerance2 = 0;
                        // Search for the other lines (from the second line to the end)
                        for(int y2 = y1 + 1; y2 < y1 + templateArray.Length; y2++) {
                            (int delta, bool match2) = SearchFor1Line(screenshotArray[y2], x1, templateArray[y2 - y1], 0);
                            tolerance2 = Math.Max(tolerance2, delta);
                            if(!match2)
                                match = false;
                        }
                        if(match) {
                            // return the middle point
                            return (x1 + offsetX + template.Width / 2, y1 + offsetY + template.Height / 2);
                        }
                    }
                }
            }
            return (0, 0);
        }

        /// <summary>
        /// Search for a .bmp image on the screen
        /// </summary>
        /// <param name="template">Bmp image to search</param>
        /// <param name="tolerance"></param>
        /// <returns>Center of the image on screen</returns>
        public (int, int) LocateOnScreenFromResource(Stream templateStream, int tolerance) {
            // Get bitmap from stream
            Bitmap template = new Bitmap(templateStream);

            // tolerance^2 = deltaR^2 + deltaG^2 + deltaB^2
            int toleranceSquared = tolerance * tolerance;

            byte[][][] GetBitmapArray(Bitmap bitmap) {
                // byte[y][x][r, g, b]
                byte[][][] bitmapArray = new byte[bitmap.Height][][];

                unsafe {
                    // Me from 2019-08-29: I don't understand either :(
                    // Me from 2019-10-24: Me neither :(
                    BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                    int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                    int heightInPixels = bitmapData.Height;
                    int widthInBytes = bitmapData.Width * bytesPerPixel;
                    byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                    // Search for each row (why I can't use Parallel.For)
                    for (int y = 0; y < heightInPixels; y++) {
                        bitmapArray[y] = new byte[bitmap.Width][];
                        byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                        for (int x = 0; x < widthInBytes; x += bytesPerPixel) {
                            bitmapArray[y][x / bytesPerPixel] = new byte[] {
                                    currentLine[x + 2],    //r
                                    currentLine[x + 1],    //g
                                    currentLine[x]         //b
                                };
                        }
                    }
                    bitmap.UnlockBits(bitmapData);
                }
                return bitmapArray;
            }

            // Check if all pixels match (only for 1 line)
            (int, bool) SearchFor1Line(byte[][] screenLine, int screenStart, byte[][] templateLine, int templateStart) {
                int maxDelta = 0;
                for (int x = 0; x < templateLine.Length; x++) {
                    // Get rgb of the corresp pixel
                    byte[] screenRGB = screenLine[x + screenStart];
                    byte[] templateRGB = templateLine[x + templateStart];
                    int delta = 0;
                    // Add 3 (rgb) squared deltas together
                    for (int i = 0; i < 3; i++) {
                        delta += (screenRGB[i] - templateRGB[i]) * (screenRGB[i] - templateRGB[i]);
                    }
                    if (delta > toleranceSquared) {
                        return (0, false);
                    }
                    maxDelta = Math.Max(maxDelta, delta);
                }
                return (maxDelta, true);
            }


            // Take screenshot
            Size screenshotSize = new Size(offsetW, offsetH);
            Bitmap screenshot = new Bitmap(offsetW, offsetH, PixelFormat.Format32bppRgb);
            Graphics graphics = Graphics.FromImage(screenshot);
            graphics.CopyFromScreen(offsetX, offsetY, 0, 0, screenshotSize, CopyPixelOperation.SourceCopy);


            // Get 2 images' array   byte[y][x][rgb]
            byte[][][] screenshotArray = GetBitmapArray(screenshot);
            byte[][][] templateArray = GetBitmapArray(template);

            // Help! foreach loop with index!!!!
            // Search for each line (screen)
            for (int y1 = 0; y1 < screenshotArray.Length - templateArray.Length; y1++) {
                // Search for each px on the line (screen)
                for (int x1 = 0; x1 < screenshotArray[y1].Length - templateArray[0].Length; x1++) {
                    // tolerance1: Tolerance on 1st line
                    //  match1: if the 1st line (screen) matches the 1st line (template)
                    (int tolerance1, bool match1) = SearchFor1Line(screenshotArray[y1], x1, templateArray[0], 0);
                    if (match1) {
                        bool match = true;
                        int tolerance2 = 0;
                        // Search for the other lines (from the second line to the end)
                        for (int y2 = y1 + 1; y2 < y1 + templateArray.Length; y2++) {
                            (int delta, bool match2) = SearchFor1Line(screenshotArray[y2], x1, templateArray[y2 - y1], 0);
                            tolerance2 = Math.Max(tolerance2, delta);
                            if (!match2)
                                match = false;
                        }
                        if (match) {
                            // return the middle point
                            return (x1 + offsetX + template.Width / 2, y1 + offsetY + template.Height / 2);
                        }
                    }
                }
            }
            return (0, 0);
        }

        /// <summary>
        /// Save screenshot to 'screenshot.bmp'
        /// </summary>
        public void SaveScreenshot() {
            Size screenshotSize = new Size(offsetW, offsetH);
            Bitmap screenshot = new Bitmap(offsetW, offsetH, PixelFormat.Format32bppRgb);

            Graphics graphics = Graphics.FromImage(screenshot);
            graphics.CopyFromScreen(offsetX, offsetY, 0, 0, screenshotSize, CopyPixelOperation.SourceCopy);

            screenshot.Save(Path.Combine(Environment.CurrentDirectory, "Screenshot.bmp"), ImageFormat.Bmp);
        }
    }
}
