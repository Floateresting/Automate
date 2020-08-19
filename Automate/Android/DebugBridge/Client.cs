﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Automate.Android.DebugBridge {
    public class Client : IDisposable {
        public static Encoding Encoding { get; set; } = Encoding.GetEncoding("iso-8859-1");
        public static int DefaultServerPort { get; set; } = 5037;

        private readonly TcpClient tc;
        private readonly NetworkStream ns;
        private readonly BinaryReader br;

        #region Constructor

        public Client() : this(new IPEndPoint(IPAddress.Loopback, Client.DefaultServerPort)) { }
        public Client(IPEndPoint p) {
            this.tc = new TcpClient();
            this.tc.Connect(p);

            this.ns = this.tc.GetStream();
            this.ns.ReadTimeout = 2000;

            this.br = new BinaryReader(this.ns, Client.Encoding);
        }
        #endregion Constructor

        public async Task<string> GetStringAsync(string s) {
            await this.WriteAsync(s);
            return this.br.ReadString();
        }

        /// <summary>
        /// Send request to adb
        /// </summary>
        /// <param name="s"></param>
        public async Task WriteAsync(string s) {
            byte[] data = s.ToAdbBytes(Client.Encoding);
            await this.ns.WriteAsync(data, 0, data.Length);
            this.EnsureSucess();
        }

        /// <summary>
        /// Read 4 bytes of data and make sure it's sucessful
        /// </summary>
        public void EnsureSucess() {
            if(Client.Encoding.GetString(this.br.ReadBytes(4)) != "OKAY") {
                Debugger.Break();
            }
        }

        #region Implementation

        public void Dispose() {
            this.tc.Dispose();
        }
        #endregion Implementation
    }
}
