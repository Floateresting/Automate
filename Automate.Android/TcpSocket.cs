using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;

namespace Automate.Android {
    public class TcpSocket : IDisposable {
        private readonly Socket socket;

        public TcpSocket(string host, int port) {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.socket.Connect(host, port);
        }

        #region Get

        internal T Get<T>(string s, Func<NetworkStream, T> handler) {
            this.Send(s);
            return handler(new NetworkStream(this.socket));
        }

        internal byte[] GetBytes(string s) {
            this.Send(s);
            return this.ReceiveBytes();
        }

        internal string GetString(string s) {
            this.Send(s);
            return this.ReceiveString();
        }
        #endregion Get

        #region Send

        internal void Send(string s) {
            this.socket.Send(Protocol.Encode(s));
            this.EnsureSucess();
        }
        #endregion Send

        #region Receive

        private byte[] ReceiveBytes(int length) {
            byte[] buffer = new byte[length];
            this.socket.Receive(buffer);
            return buffer;
        }

        private byte[] ReceiveBytes() {
            using NetworkStream ns = new NetworkStream(this.socket);
            int i;
            List<byte> bs = new List<byte>();
            while((i = ns.ReadByte()) != -1) {
                bs.Add((byte)i);
            }
            return bs.ToArray();
        }

        private string ReceiveString(int length) {
            return Protocol.Decode(this.ReceiveBytes(length));
        }

        private string ReceiveString() {
            // The first 4 bytes indicates the length of the following string
            int length = int.Parse(this.ReceiveString(4), NumberStyles.HexNumber);
            return this.ReceiveString(length);
        }
        #endregion Receive

        private void EnsureSucess() {
            string status = this.ReceiveString(4);
            if(status != Protocol.OKAY) {
                throw new Exception("Not okay");
            }
        }

        public void Dispose() {
            this.socket.Dispose();
        }
    }
}
