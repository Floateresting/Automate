using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Automate {
    public class Application {
        // Issue with the "Bounds"
        public double ratio = (double)Screen.PrimaryScreen.Bounds.Width / 1920;

        // Set focus to this app
        [DllImport("user32", SetLastError = true)]
        static extern void SwitchToThisWindow(IntPtr hwnd, bool turnOn);

        // Change positon and size
        [DllImport("user32", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hwnd, int x, int y, int w, int h, bool repaint);

        public string processName;
        public IntPtr processHwnd;

        public Application(string processName) {
            this.processName = processName;
            // search for all the processes with this name (processName)
            foreach(Process process in Process.GetProcessesByName(processName)) {
                // if this process has a title
                if(!String.IsNullOrEmpty(process.MainWindowTitle)) {
                    this.processHwnd = process.MainWindowHandle;
                }
            }
        }

        public void Switch() {
            SwitchToThisWindow(processHwnd, true);
        }

        public void Resize(int x, int y, int w, int h) {
            MoveWindow(processHwnd, (int)(x * ratio), (int)(y * ratio), (int)(w * ratio), (int)(h * ratio), true);
        }

    }
}
