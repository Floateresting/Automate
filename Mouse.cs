using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;

namespace Automate {
    public class Mouse {
        // Issue with the "Bounds"
        public double ratio = (double)Screen.PrimaryScreen.Bounds.Width / 1920;

        // Create MousePoint datatype
        [StructLayout(LayoutKind.Sequential)]
        internal struct MousePoint {
            public int X, Y;
        }

        // Move cursor to (x, y)
        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);

        // Perform clicks
        [DllImport("user32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        // Get cursor position
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref MousePoint point);

        private const int MOUSE_LEFT_DOWN = 0x02;
        private const int MOUSE_LEFT_UP = 0x04;
        // private const int MOUSE_RIGHT_DOWN = 0x08;
        // private const int MOUSE_RIGHT_UP = 0x10;

        /// <summary>
        /// Move cursor to a position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MoveTo(int x, int y) {
            SetCursorPos((int)(x * ratio), (int)(y * ratio));
        }

        public void MoveTo(double x, double y) {
            SetCursorPos((int)(x * ratio), (int)(y * ratio));
        }

        /// <summary>
        /// Move cursor to a position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MoveTo(Point point) {
            SetCursorPos((int)(point.X* ratio), (int)(point.Y* ratio));
        }

        /// <summary>
        /// Perform a left click
        /// </summary>
        public void Click() {
            mouse_event(MOUSE_LEFT_DOWN | MOUSE_LEFT_UP, 0, 0, 0, 0);
        }

        public void ClickAt(int x, int y) {
            MoveTo(x,y);
            Click();
        }

        public void ClickAt(double x, double y) {
            MoveTo(x, y);
            Click();
        }


        public void ClickAt(Point point) {
            MoveTo(point);
            Click();
        }

        /// <summary>
        /// Drag the mouse at the x axis
        /// </summary>
        /// <param name="from">Starting point</param>
        /// <param name="px"></param>
        /// <param name="pxPerSec">Maximum 5</param>
        public void DragH(Point from, int px, int pxPerSec) {
            MoveTo(from);
            mouse_event(MOUSE_LEFT_DOWN, 0, 0, 0, 0);
            for(int i = 1; i <= Math.Abs(px); i += pxPerSec) {
                // if px is negative, move to left, else to right
                MoveTo(from.X + (px > 0 ? i : -i), from.Y);
                Thread.Sleep(1);
            }
            mouse_event(MOUSE_LEFT_UP, 0, 0, 0, 0);
        }

        /// <summary>
        /// Drag the mouse at the y axis
        /// </summary>
        /// <param name="from">Starting point</param>
        /// <param name="px"></param>
        /// <param name="pxPerSec">Maximum 5</param>
        public void DragV(Point from, int px, int pxPerSec) {
            MoveTo(from);
            mouse_event(MOUSE_LEFT_DOWN, 0, 0, 0, 0);
            for(int i = 1; i <= Math.Abs(px); i += pxPerSec) {
                // if px is negative, move to left, else to right
                MoveTo(from.X, from.Y + (px > 0 ? i : -i));
                Thread.Sleep(1);
            }
            mouse_event(MOUSE_LEFT_UP, 0, 0, 0, 0);
        }


        /// <summary>
        /// Get current cursor position
        /// </summary>
        /// <returns>(x, y)</returns>
        public Point GetMousePosition() {
            MousePoint mousePoint = new MousePoint();
            GetCursorPos(ref mousePoint);
            return new Point((int)(mousePoint.X / ratio), (int)(mousePoint.Y / ratio));
        }
    }
}
