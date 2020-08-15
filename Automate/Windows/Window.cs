using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Automate {
    /// <summary>
    /// Class for searching image on screen, take screenshots
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
        #endregion
    }
}
