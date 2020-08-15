using Microsoft.Win32;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Automate.Android.DebugBridge {
    public class Client : IDisposable {
        public static Encoding Encoding { get; set; } = Encoding.GetEncoding("iso-8859-1");
        public static int DefaultServerPort { get; set; } = 5037;

        private readonly TcpClient tc;
        private readonly NetworkStream ns;

        #region Constructor

        public Client() : this(new IPEndPoint(IPAddress.Loopback, Client.DefaultServerPort)) { }
        public Client(IPEndPoint p) {
            this.tc = new TcpClient();
            this.tc.Connect(p);
            this.ns = tc.GetStream();
            this.ns.ReadTimeout = 2000;
        }
        #endregion Constructor

        public void Write(string s) {
            byte[] data = s.ToAdbBytes(Client.Encoding);
            this.ns.Write(data, 0, data.Length);
        }

        public string ReadString() {
            using StreamReader sr = new StreamReader(this.ns, Client.Encoding);
            return sr.ReadToEnd();
        }

        #region Implementation

        public void Dispose() {
            tc.Dispose();
        } 
        #endregion Implementation
    }
}
