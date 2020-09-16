using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace Automate.Android {
    public class Device {
        public string Serial { get; set; }
        private readonly Host host;

        public Device(Host h, string serial) {
            this.host = h;
            this.Serial = serial;
        }

        #region Shell

        /// <summary>
        /// Execute a shell command without handling the reply
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public byte[] Shell(string s) {
            using TcpSocket ts = this.host.CreateConnection(this.Serial);
            return ts.GetBytes($"shell:{s}");
        }

        /// <summary>
        /// Execute a shell command with handling the reply
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public T Shell<T>(string s, Func<NetworkStream, T> handler) {
            using TcpSocket ts = this.host.CreateConnection(this.Serial);
            return ts.Get($"shell:{s}", handler);
        }

        /// <summary>
        /// Execute 'screencap' and return the RAW reply
        /// </summary>
        /// <returns>byte[x,y][] of {r, g, b, a}</returns>
        public ScreenCapture Screencap() {
            return this.Shell("screencap", ns => ScreenCapture.FromStream(ns));
        }

        /// <summary>
        /// Execute 'screencap -p' and create a png file
        /// </summary>
        /// <param name="pngpath"></param>
        /// <returns></returns>
        public bool Screencap(string pngpath) {
            return this.Shell("screencap -p", ns => {
                int b;
                List<byte> bs = new List<byte>();
                while((b = ns.ReadByte()) != -1) {
                    bs.Add((byte)b);
                }
                File.WriteAllBytes(pngpath, bs.ToArray());
                return true;
            });
        }

        public byte[] InputTap(int x, int y) {
            return this.Shell($"input tap {x} {y}");
        }

        public byte[] InputTap(Point p) {
            return this.InputTap(p.X, p.Y);
        }

        /// <summary>
        /// Swipe from (x1, y1) to (x2, y2) in {duration} ms
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="duration">Milliseconds</param>
        /// <returns></returns>
        public byte[] InputSwipe(int x1, int y1, int x2, int y2, int? duration = null) {
            return this.Shell($"input swipe {x1} {y1} {x2} {y2} {duration}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="duration">Milliseconds</param>
        /// <returns></returns>
        public byte[] InputSwipe(Point from, Point to, int? duration = null) {
            return this.InputSwipe(from.X, from.Y, to.X, to.Y, duration);
        }
        #endregion Shell
    }
}
