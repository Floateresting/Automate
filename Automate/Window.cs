using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Automate {
    /// <summary>
    /// Class for searching image on screen, take screenshots
    /// Should I make this class static? IDK
    /// </summary>
    public class Window {
        #region Dll Imports

        private struct DEVMODE {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);
        #endregion

        #region Private Members

        /// <summary>
        /// Left-top corner of the screen
        /// </summary>
        private Point position = new Point();

        /// <summary>
        /// Resolution of the screen
        /// </summary>
        private Size screenshotSize = Window.GetMonitorResolution();
        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="x">X position of searching area</param>
        /// <param name="y">Y position of searching area</param>
        /// <param name="w">Width of searching area</param>
        /// <param name="h">Height of searching area</param>
        public Window(int x, int y, int w, int h) {
            this.position = new Point(x, y);
            this.screenshotSize = new Size(w, h);
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="point">Position of searching area</param>
        /// <param name="size">Size of searching area</param>
        public Window(Point point, Size size) {
            this.position = point;
            this.screenshotSize = size;
        }

        /// <summary>
        /// Default Contructor (only for lazy people)
        /// </summary>
        public Window() { }
        #endregion

        #region Public Methods

        /// <summary>
        /// Search for a .bmp image on the screen
        /// </summary>
        /// <param name="templatePath">Path to the template image</param>
        /// <param name="tolerance">Color difference between pixels, which is Sqrt(r^2 + g^2 + b^2)</param>
        /// <param name="result">First matched result, new <see cref="System.Drawing.Point()"/> if not found</param>
        /// <param name="position">(Optimal) Position of top-left corner of the screenshot</param>
        /// <param name="screenshotSize">Size of the screenshot</param>
        /// <returns>True if the image was found</returns>
        public bool LocateOnScreen(string templatePath, out Point result, int tolerance = 0, Point? position = null, Size? screenshotSize = null) {
            #region Before Searching

            // Only supports bitmap image
            if(!templatePath.EndsWith(".bmp")) { throw new NotImplementedException($"{templatePath} is not a bitmap image"); }

            // File not found
            if(!File.Exists(templatePath)) { throw new FileNotFoundException($"templatePath not valid, can't find '{templatePath}'"); }

            // if the user didn't give the parameters
            if(screenshotSize == null) {
                screenshotSize = this.screenshotSize;
            }
            if(position == null) {
                position = this.position;
            }
            #endregion

            using(Bitmap template = new Bitmap(templatePath)) {

                // tolerance^2 = deltaR^2 + deltaG^2 + deltaB^2
                int toleranceSquared = tolerance * tolerance;

                // Get 2 images' array      byte[y][x][rgb]
                byte[][][] screenshotArray = Window.GetBitmapArray(Window.TakeScreenshot((Point)position, (Size)screenshotSize));
                byte[][][] templateArray = Window.GetBitmapArray(template);

                // Help! foreach loop with index!!!!
                // Search for each line (screen)
                for(int y1 = 0; y1 < screenshotArray.Length - templateArray.Length; y1++) {
                    // Search for each px on the line (screen)
                    for(int x1 = 0; x1 < screenshotArray[y1].Length - templateArray[0].Length; x1++) {
                        // match1: if the 1st line (screen) matches the 1st line (template)
                        bool match1 = SearchFor1Line(screenshotArray[y1], x1, templateArray[0], 0, toleranceSquared, out _);
                        if(match1) {
                            bool match = true;
                            int tolerance2 = 0;
                            // Search for the other lines (from the second line to the end)
                            for(int y2 = y1 + 1; y2 < y1 + templateArray.Length; y2++) {
                                bool match2 = SearchFor1Line(screenshotArray[y2], x1, templateArray[y2 - y1], 0, toleranceSquared, out int delta);
                                tolerance2 = Math.Max(tolerance2, delta);
                                if(!match2) {
                                    match = false;
                                }
                            }
                            if(match) {
                                // return the middle Point
                                result = new Point(x1 + ((Point)position).X + template.Width / 2, y1 + ((Point)position).Y + template.Height / 2);
                                return true;
                            }
                        }
                    }
                }
            }

            // Can't find image
            result = new Point();
            return false;
        }

        /// <summary>
        /// Search for a .bmp image on the screen, and get it's actual tolerance
        /// </summary>
        /// <param name="templatePath">Path to the template image</param>
        /// <param name="result">First matched result, new <see cref="System.Drawing.Point()"/> if not found</param>
        /// <param name="actualTolerance">Actual tolerance found</param>
        /// <param name="estimatedTolerence">Color difference between pixels, which is Sqrt(r^2 + g^2 + b^2)</param>
        /// <param name="position">(Optimal) Position of top-left corner of the screenshot</param>
        /// <param name="screenshotSize">Size of the screenshot</param>
        /// <returns>True if the image was found</returns>
        public bool LocateOnScreen(string templatePath, out Point result, out double actualTolerance, int estimatedTolerence = 0, Point? position = null, Size? screenshotSize = null) {
            #region Before Searching

            // Only supports bitmap image
            if(!templatePath.EndsWith(".bmp")) { throw new NotImplementedException($"{templatePath} is not a bitmap image"); }

            // File not found
            if(!File.Exists(templatePath)) { throw new FileNotFoundException($"templatePath not valid, can't find '{templatePath}'"); }

            // if the user didn't give the parameters
            if(screenshotSize == null) {
                screenshotSize = this.screenshotSize;
            }
            if(position == null) {
                position = this.position;
            }
            #endregion

            using(Bitmap template = new Bitmap(templatePath)) {

                // tolerance^2 = deltaR^2 + deltaG^2 + deltaB^2
                int toleranceSquared = estimatedTolerence * estimatedTolerence;

                // Get 2 images' array      byte[y][x][rgb]
                byte[][][] screenshotArray = Window.GetBitmapArray(Window.TakeScreenshot((Point)position, (Size)screenshotSize));
                byte[][][] templateArray = Window.GetBitmapArray(template);

                // Help! foreach loop with index!!!!
                // Search for each line (screen)
                for(int y1 = 0; y1 < screenshotArray.Length - templateArray.Length; y1++) {
                    // Search for each px on the line (screen)
                    for(int x1 = 0; x1 < screenshotArray[y1].Length - templateArray[0].Length; x1++) {
                        // match1: if the 1st line (screen) matches the 1st line (template)
                        bool match1 = SearchFor1Line(screenshotArray[y1], x1, templateArray[0], 0, toleranceSquared, out int tolerance1);
                        if(match1) {
                            bool match = true;
                            int tolerance2 = 0;
                            // Search for the other lines (from the second line to the end)
                            for(int y2 = y1 + 1; y2 < y1 + templateArray.Length; y2++) {
                                bool match2 = SearchFor1Line(screenshotArray[y2], x1, templateArray[y2 - y1], 0, toleranceSquared, out int delta);
                                tolerance2 = Math.Max(tolerance2, delta);
                                if(!match2) {
                                    match = false;
                                }
                            }
                            if(match) {
                                actualTolerance = Math.Sqrt(tolerance1);

                                // return the middle Point
                                result = new Point(x1 + ((Point)position).X + template.Width / 2, y1 + ((Point)position).Y + template.Height / 2);
                                return true;
                            }
                        }
                    }
                }
            }

            // Can't find image
            actualTolerance = 0;
            result = new Point();
            return false;
        }

        /// <summary>
        /// Search for all matches template on screen
        /// </summary>
        /// <param name="templatePath">Path to the template image</param>
        /// <param name="minDistance">Minimum distance between each found template in px</param>
        /// <param name="tolerance">Color difference between pixels, which is Sqrt(r^2 + g^2 + b^2)</param>
        /// <param name="results">All matched results, new <see cref="System.Collections.Generic.List{T}()"/> if nothing found</param>
        /// <param name="position">(Optimal) Position of top-left corner of the screenshot</param>
        /// <param name="screenshotSize">Size of the screenshot</param>
        /// <returns></returns>
        public bool LocateAllOnScreen(string templatePath, int minDistance, out List<Point> results, int tolerance = 0, Point? position = null, Size? screenshotSize = null) {
            #region Before Searching

            // Only supports bitmap image
            if(!templatePath.EndsWith(".bmp")) { throw new NotImplementedException($"{templatePath} is not a bitmap image"); }

            // File not found
            if(!File.Exists(templatePath)) { throw new FileNotFoundException($"templatePath not valid, can't find '{templatePath}'"); }

            // if the user didn't give the parameters
            if(screenshotSize == null) {
                screenshotSize = this.screenshotSize;
            }
            if(position == null) {
                position = this.position;
            }

            // Initialize variable
            results = new List<Point>();
            #endregion

            #region Searching

            using Bitmap template = new Bitmap(templatePath);

            // tolerance^2 = deltaR^2 + deltaG^2 + deltaB^2
            int toleranceSquared = tolerance * tolerance;


            // Get 2 images' array   byte[y][x][rgb]
            byte[][][] screenshotArray = Window.GetBitmapArray(Window.TakeScreenshot((Point)position, (Size)screenshotSize));
            byte[][][] templateArray = Window.GetBitmapArray(template);

            // Help! foreach loop with index!!!!
            // Search for each line (screen)
            for(int y1 = 0; y1 < screenshotArray.Length - templateArray.Length; y1++) {
                // I don't know why Visual Studio give me warning, but if I change it, it won't work
                bool match = true;
                // Search for each px on the line (screen)
                for(int x1 = 0; x1 < screenshotArray[y1].Length - templateArray[0].Length; x1++) {
                    bool skip = false;
                    // foreach found points...
                    foreach(Point foundPoint in results) {
                        // if current x1 is between the found template
                        if(foundPoint.X - ((Point)position).X - (templateArray[0].Length + minDistance) / 2f <= x1 && x1 <= foundPoint.X - ((Point)position).X + (templateArray[0].Length + minDistance) / 2f) {
                            // if current y1 is between the found template
                            if(foundPoint.Y - ((Point)position).Y - (templateArray.Length + minDistance) / 2f <= y1 && y1 <= foundPoint.Y - ((Point)position).Y + (templateArray.Length + minDistance) / 2f) {
                                // skip this location
                                skip = true;
                                break;
                            }
                        }
                    }
                    if(skip) {
                        continue;
                    }

                    // match1: if the 1st line (screen) matches the 1st line (template)
                    bool match1 = SearchFor1Line(screenshotArray[y1], x1, templateArray[0], 0, toleranceSquared, out _);
                    if(match1) {
                        match = true;
                        int tolerance2 = 0;
                        // Search for the other lines (from the second line to the end)
                        for(int y2 = y1 + 1; y2 < y1 + templateArray.Length; y2++) {
                            bool match2 = SearchFor1Line(screenshotArray[y2], x1, templateArray[y2 - y1], 0, toleranceSquared, out int delta);
                            tolerance2 = Math.Max(tolerance2, delta);
                            if(!match2) {
                                match = false;
                            }
                        }
                        if(match) {
                            // Add the middle Point to the list
                            results.Add(new Point(x1 + ((Point)position).X + template.Width / 2, y1 + ((Point)position).X + template.Height / 2));
                            // x: Start at after the found template
                            x1 += templateArray[0].Length;
                        }
                    }
                }
            }
            #endregion            

            return results.Count > 0;
        }

        /// <summary>
        /// Take a screenshot and save it as a bitmap file
        /// </summary>
        /// <param name="path">Output path</param>
        /// <param name="position">Left-corner position of the screenshot</param>
        /// <param name="screenshotSize">Size (or resolution) of the screenshot</param>
        public void SaveScreenshot(string path = "Screenshot.bmp", Point? position = null, Size? screenshotSize = null) {
            // if the user didn't give the parameters
            if(screenshotSize == null) {
                screenshotSize = this.screenshotSize;
            }
            if(position == null) {
                position = this.position;
            }

            if(!path.EndsWith(".bmp"
)) {
                path += ".bmp";
            }
            Window.TakeScreenshot((Point)position, (Size)screenshotSize)
                .Save(path, ImageFormat.Bmp);
        }
        #endregion

        #region Private helper methods

        /// <summary>
        /// Get monitor's resolution
        /// </summary>
        /// <returns></returns>
        private static Size GetMonitorResolution() {
            // Using some dark magic to get monitor's resolution
            const int ENUM_CURRENT_SETTINGS = -1;
            DEVMODE devMode = default;
            devMode.dmSize = (short)Marshal.SizeOf(devMode);
            EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode);

            return new Size(devMode.dmPelsWidth, devMode.dmPelsHeight);
        }

        /// <summary>
        /// Take screenshot from specified location
        /// </summary>
        /// <param name="position">top-left corner position of the screenshot</param>
        /// <param name="screenshotSize">size (or resolution) of the screenshot</param>
        /// <returns>Bitmap image of the screenshot</returns>
        private static Bitmap TakeScreenshot(Point position, Size screenshotSize) {
            Bitmap screenshot = new Bitmap(screenshotSize.Width, screenshotSize.Height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(screenshot);
            graphics.CopyFromScreen(position.X, position.Y, 0, 0, screenshotSize, CopyPixelOperation.SourceCopy);

            return screenshot;
        }

        /// <summary>
        /// Convert a bmp image to rgb array
        /// </summary>
        /// <param name="bitmap">Bitmap image to convert</param>
        /// <returns>byte[y][x][r, g, b]</returns>
        private static byte[][][] GetBitmapArray(Bitmap bitmap) {
            // byte[y][x][r, g, b]
            byte[][][] bitmapArray = new byte[bitmap.Height][][];

            unsafe {
                // Me from 2019-08-29: I don't understand anything :(
                // Me from 2019-10-24: Me neither :(
                // Me from 2019-12-13: ME neither :(
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

        /// <summary>
        /// Check if all pixels match (only for 1 line)
        /// </summary>
        /// <param name="screenLine">rgb array of a pixel line from screen</param>
        /// <param name="screenStart">position of where to start</param>
        /// <param name="templateLine">rgb array of a pixel line from template</param>
        /// <param name="templateStart">position of where to start</param>
        /// <param name="toleranceSquared"></param>
        /// <param name="actualTolerance">Actual tolerance found</param>
        /// <returns>True if the line matches</returns>
        private static bool SearchFor1Line(byte[][] screenLine, int screenStart, byte[][] templateLine, int templateStart, int toleranceSquared, out int actualTolerance) {
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
                    actualTolerance = 0;
                    return false;
                }
                maxDelta = Math.Max(maxDelta, delta);
            }
            actualTolerance = maxDelta;
            return true;
        }
        #endregion
    }
}
