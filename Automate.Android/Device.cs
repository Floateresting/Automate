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
        /// <seealso href="https://stackoverflow.com/a/32733228"/>
        /// <seealso href="https://android.googlesource.com/platform/frameworks/base/+/android-4.3_r2.3/cmds/screencap/screencap.cpp#191"/>
        /// <returns>byte[x,y][] of {r, g, b, a}</returns>
        public byte[,][] Screencap() {
            return this.Shell("screencap", ns => {
                using BinaryReader br = new BinaryReader(ns);

                #region Read Raw Data

                // width, height, pixel format
                int w = br.ReadInt32();
                int h = br.ReadInt32();
                int f = br.ReadInt32();

                if(f != 1) throw new Exception("This is not rgba_8888 format");

                byte[,][] raw = new byte[w, h][];
                for(int y = 0; y < h; y++) {
                    for(int x = 0; x < w; x++) {
                        raw[x, y] = new byte[] {
                        br.ReadByte(), // r
                        br.ReadByte(), // g
                        br.ReadByte(), // b
                        br.ReadByte(), // a
                    };
                    }
                }
                #endregion Read Raw Data
                return raw;
            });
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
