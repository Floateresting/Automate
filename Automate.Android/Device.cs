using System;
using System.IO;
using System.Net.Sockets;

namespace Automate.Android {
    public class Device {
        public string Serial { get; set; }
        private readonly Host host;

        public Device(Host h, string serial) {
            this.host = h;
            this.Serial = serial;
        }


        /// <summary>
        /// Execute 'screencap' and return the RAW reply
        /// </summary>
        /// <returns>byte[x,y][] of {r, g, b, a}</returns>
        public ScreenCapture Screencap() {
            return this.Shell("screencap", ns => ScreenCapture.FromStream(ns));
        }

        /// <summary>
        /// Execute 'input tap'
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public byte[] InputTap(int x, int y) {
            return this.Shell($"input tap {x} {y}");
        }

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
    }
}
