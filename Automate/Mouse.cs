using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace Automate {
    /// <summary>
    /// Class for get mouse position, set mouse position, perform clicks or drags
    /// </summary>
    public class Mouse {
        #region Dll Imports

        // Move cursor to (x, y)
        [DllImport("user32")]
        private static extern int SetCursorPos(int x, int y);

        // Perform clicks
        [DllImport("user32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        // Get cursor position
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(ref Point point);
        #endregion

        #region Private Members

        /// <summary>
        /// Scaling factor (e.g. 125%)
        /// </summary>
        private readonly float scaling = Utils.GetScalingFactor();

        private const int MOUSE_LEFT_DOWN = 0x02;
        private const int MOUSE_LEFT_UP = 0x04;
        // private const int MOUSE_RIGHT_DOWN = 0x08;
        // private const int MOUSE_RIGHT_UP = 0x10;
        #endregion

        // IntelliCode told me to use static methods,
        // but I tested that static methods are slower. 
        #region Public Methods

        #region Mouse Movement

        /// <summary>
        /// Move cursor to a position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MoveTo(int x, int y) {
            SetCursorPos((int)(x * this.scaling), (int)(y * this.scaling));
        }

        /// <summary>
        /// Move cursor to a position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MoveTo(Point point) {
            SetCursorPos((int)(point.X * this.scaling), (int)(point.Y * this.scaling));
        }
        #endregion

        #region Mouse Click

        /// <summary>
        /// Perform a left click
        /// </summary>
        public void Click() {
            mouse_event(MOUSE_LEFT_DOWN | MOUSE_LEFT_UP, 0, 0, 0, 0);
        }

        public void ClickAt(int x, int y) {
            SetCursorPos((int)(x * this.scaling), (int)(y * this.scaling));
            mouse_event(MOUSE_LEFT_DOWN | MOUSE_LEFT_UP, 0, 0, 0, 0);
        }

        public void ClickAt(Point point) {
            SetCursorPos((int)(point.X * this.scaling), (int)(point.Y * this.scaling));
            mouse_event(MOUSE_LEFT_DOWN | MOUSE_LEFT_UP, 0, 0, 0, 0);
        }
        #endregion

        #region Mouse Drag

        /// <summary>
        /// Drag the mouse at the x axis
        /// </summary>
        /// <param name="from">Starting point</param>
        /// <param name="px">Relative pixel count (negative if towards left, positive if towards right)</param>
        /// <param name="pxPerSec">Maximum 5</param>
        public void DragH(Point from, int px, int pxPerSec) {
            // Maximum 5, else doesn't work
            pxPerSec = pxPerSec > 5 ? 5 : pxPerSec;

            SetCursorPos((int)(from.X * this.scaling), (int)(from.Y * this.scaling));

            mouse_event(MOUSE_LEFT_DOWN, 0, 0, 0, 0);
            for(int i = 1; i <= Math.Abs(px); i += pxPerSec) {
                // if px is negative, move to left, else to right
                SetCursorPos((int)((from.X + (px > 0 ? i : -i)) * this.scaling), (int)(from.Y * this.scaling));
                Thread.Sleep(1);
            }
            mouse_event(MOUSE_LEFT_UP, 0, 0, 0, 0);
        }

        /// <summary>
        /// Drag the mouse at the y axis
        /// </summary>
        /// <param name="from">Starting point</param>
        /// <param name="px">Relative pixel count (negative if towards up, positive if towards down)</param>
        /// <param name="pxPerSec">Maximum 5</param>
        public void DragV(Point from, int px, int pxPerSec) {
            // Maximum 5, else does't work
            pxPerSec = pxPerSec > 5 ? 5 : pxPerSec;

            SetCursorPos((int)(from.X * this.scaling), (int)(from.Y * this.scaling));

            mouse_event(MOUSE_LEFT_DOWN, 0, 0, 0, 0);
            for(int i = 1; i <= Math.Abs(px); i += pxPerSec) {
                // if px is negative, move to left, else to right
                SetCursorPos((int)(from.X * this.scaling), (int)((from.Y + (px > 0 ? i : -i)) * this.scaling));
                Thread.Sleep(1);
            }
            mouse_event(MOUSE_LEFT_UP, 0, 0, 0, 0);
        }
        #endregion

        /// <summary>
        /// Get current cursor position
        /// </summary>
        /// <returns>(x, y)</returns>
        public Point GetMousePosition() {
            Point point = new Point();
            GetCursorPos(ref point);
            return new Point((int)(point.X * this.scaling), (int)(point.Y * this.scaling));
        }
        #endregion
    }
}
