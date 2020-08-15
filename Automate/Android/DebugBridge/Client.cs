using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

        public void Write(string s) {
            byte[] data = s.ToAdbBytes(Client.Encoding);
            this.ns.Write(data, 0, data.Length);
        }

        public string ReadString() {
            this.EnsureSucess();
            return this.br.ReadString();
        }

        public void EnsureSucess() {
            if(Client.Encoding.GetString(this.br.ReadBytes(4)) != "OKAY") {
                Debugger.Break();
            }
        }

        public void SetDevice(Device d) {
            if(d != null) {
                this.Write($"host:transport:{d.Serial}");

            }
        }
        #region Implementation

        public void Dispose() {
            this.tc.Dispose();
        }
        #endregion Implementation
    }
}
