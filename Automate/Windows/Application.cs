using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Automate {
    /// <summary>
    /// Class for set focus, resize or move an application
    /// </summary>
    public class Application {

        /// <summary>
        /// Scaling factor (e.g. 125%)
        /// </summary>
        private readonly float scaling = Utils.GetScalingFactor();

        // Set focus to this app
        [DllImport("user32", SetLastError = true)]
        private static extern void SwitchToThisWindow(IntPtr hwnd, bool turnOn);

        // Change positon and size
        [DllImport("user32", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int w, int h, bool repaint);

        private readonly IntPtr processHwnd;

        public Application(string processName) {
            // search for all the processes with this name (processName)
            foreach(Process process in Process.GetProcessesByName(processName)) {
                // if this process has a title
                if(!String.IsNullOrEmpty(process.MainWindowTitle)) {
                    this.processHwnd = process.MainWindowHandle;
                }
            }
        }

        /// <summary>
        /// Set focus of this app
        /// </summary>
        public void SetFocus() {
            SwitchToThisWindow(processHwnd, true);
        }

        /// <summary>
        /// Resize/Move the window
        /// </summary>
        /// <param name="x">New position of the left side of the window</param>
        /// <param name="y">New position of the top of the window</param>
        /// <param name="w">New width of the window</param>
        /// <param name="h">New height of the window</param>
        /// <returns>If the function succeeds, the return value is nonzero</returns>
        public bool ResizeAndMove(int x, int y, int w, int h) {
            return MoveWindow(processHwnd,
                (int)(x / this.scaling),
                (int)(y / this.scaling),
                (int)(w / this.scaling),
                (int)(h / this.scaling),
                true);
        }

        /// <summary>
        /// Resize/Move the window
        /// </summary>
        /// <param name="point">Left-top corner position of the window</param>
        /// <param name="size">Size of the window</param>
        /// <returns>If the function succeeds, the return value is nonzero</returns>
        public bool ResizeAndMove(Point point, Size size) {
            return MoveWindow(processHwnd,
                (int)(point.X / this.scaling),
                (int)(point.Y / this.scaling),
                (int)(size.Width / this.scaling),
                (int)(size.Height / this.scaling),
                true);
        }

    }
}
