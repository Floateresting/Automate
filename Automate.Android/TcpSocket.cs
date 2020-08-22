using System;
using System.Globalization;
using System.Net.Sockets;

namespace Automate.Android {
    public class TcpSocket : IDisposable {
        private readonly Socket s;

        public TcpSocket(string host, int port) {
            this.s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.s.Connect(host, port);
        }


        #region Public

        public string GetString() {
            int length = int.Parse(this.ReceiveString(4), NumberStyles.HexNumber);
            return this.ReceiveString(length);
        }

        public bool CheckStatus() {
            string status = this.ReceiveString(4);
            if(status != Protocol.OKAY) {
                throw new Exception("Not okay");
            }
            return true;
        }

        public bool Send(string s) {
            this.s.Send(Protocol.Encode(s));
            return this.CheckStatus();
        }
        #endregion Public

        #region Private

        private byte[] Receive(int length) {
            byte[] buffer = new byte[length];
            this.s.Receive(buffer);
            return buffer;
        }

        private string ReceiveString(int length) {
            return Protocol.Decode(this.Receive(length));
        }
        #endregion Private


        public void Dispose() {
            this.s.Dispose();
        }
    }
}
