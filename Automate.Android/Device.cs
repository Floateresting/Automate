﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Automate.Android {
    public class Device {
        public string Serial { get; set; }
        private readonly Client c;
        public Device(Client c, string serial) {
            this.c = c;
            this.Serial = serial;
        }


        /// <summary>
        /// Execute 'screencap' and return the RAW value
        /// </summary>
        /// <seealso href="https://stackoverflow.com/a/32733228"/>
        /// <seealso href="https://android.googlesource.com/platform/frameworks/base/+/android-4.3_r2.3/cmds/screencap/screencap.cpp#191"/>
        /// <returns>byte[x,y][] of {r, g, b, a}</returns>
        public byte[,][] Screencap() {
            return this.Shell("screencap", ts => {
                using NetworkStream ns = new NetworkStream(ts.Socket);
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

        public byte[] InputTap(int x, int y) {
            return this.Shell($"input tap {x} {y}");
        }

        public byte[] Shell(string s) {
            using TcpSocket ts = this.CreateSocket();
            ts.Send($"shell:{s}");
            return ts.ReceiveAll();
        }

        public T Shell<T>(string s, Func<TcpSocket, T> handler) {
            using TcpSocket ts = this.c.CreateSocket();
            ts.Send($"shell:{s}");
            return handler(ts);
        }

        private TcpSocket CreateSocket(bool transport = true) {
            TcpSocket ts = this.c.CreateSocket();
            if(transport) {
                ts.Send($"host:transport:{this.Serial}");
            }
            return ts;
        }
    }
}
