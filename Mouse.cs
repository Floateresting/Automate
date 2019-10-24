using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace Kyuidime {
    class Mouse {
        // issue with the "Bounds"
        public double ratio = (double)Screen.PrimaryScreen.Bounds.Width / 1920;

        // create MousePoint datatype
        [StructLayout(LayoutKind.Sequential)]
        internal struct MousePoint {
            public int X, Y;
        }

        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);

        [DllImport("user32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref MousePoint point);

        private const int MOUSE_LEFT_DOWN = 0x02;
        private const int MOUSE_LEFT_UP = 0x04;
        // private const int MOUSE_RIGHT_DOWN = 0x08;
        // private const int MOUSE_RIGHT_UP = 0x10;

        public void MoveTo(Point point) {
            SetCursorPos((int)(point.X * ratio), (int)(point.Y * ratio));
        }

        public void Click() {
            mouse_event(MOUSE_LEFT_DOWN | MOUSE_LEFT_UP, 0, 0, 0, 0);
        }

        public void Drag(char axis, Point center, int pixels) {
            void MoveAtX(Point start) {
                // go from -1 to -100 || from 1 to 100, should work I guess?
                for(int i = pixels / Math.Abs(pixels); Math.Abs(i) <= Math.Abs(pixels); i += pixels / Math.Abs(pixels)) {
                    MoveTo(new Point(start.X+i, start.Y));
                    Thread.Sleep(5);
                }
            }
            
            //void MoveAtY(Point start) {
            //    // go from -1 to -100 || from 1 to 100, should work I guess? ( I...I didn't copy/paste or anything!! )
            //    for(int i = pixels / Math.Abs(pixels); Math.Abs(i) <= Math.Abs(pixels); i += pixels / Math.Abs(pixels)) {
            //        MoveTo(new Point(start.X, start.Y+i));
            //    }
            //}

            Point startpoint;
            if(axis=='x') {
                startpoint = new Point(center.X - pixels / 2, center.Y);
                MoveTo(startpoint);
                mouse_event(MOUSE_LEFT_DOWN, 0, 0, 0, 0);
                MoveAtX(startpoint);
            }
            mouse_event(MOUSE_LEFT_UP, 0, 0, 0, 0);
        }

        public Point GetMousePosition() {
            MousePoint mousePoint = new MousePoint();
            GetCursorPos(ref mousePoint);
            return new Point((int)(mousePoint.X / ratio), (int)(mousePoint.Y / ratio));
        }
    }
}
