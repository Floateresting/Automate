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

        /// <summary>
        /// Send string, check status, receive and handle the reply
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        internal T Get<T>(string s, Func<NetworkStream, T> handler) {
            this.Send(s);
            return handler(new NetworkStream(this.socket));
        }

        /// <summary>
        /// Send string, check status, receive the reply as <see cref="byte[]"/>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        internal byte[] GetBytes(string s) {
            this.Send(s);
            return this.ReceiveBytes();
        }

        /// <summary>
        /// Send string, check status, receive the reply as <see cref="string"/>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        internal string GetString(string s) {
            this.Send(s);
            return this.ReceiveString();
        }
        #endregion Get

        #region Send

        /// <summary>
        /// Send string, check status without receiving
        /// </summary>
        /// <param name="s"></param>
        internal void Send(string s) {
            this.socket.Send(Protocol.Encode(s));
            this.EnsureSucess();
        }
        #endregion Send

        #region Receive

        /// <summary>
        /// Receive a specific number of bytes
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private byte[] ReceiveBytes(int length) {
            byte[] buffer = new byte[length];
            this.socket.Receive(buffer);
            return buffer;
        }

        /// <summary>
        /// Receive all bytes until end of stream
        /// </summary>
        /// <returns></returns>
        private byte[] ReceiveBytes() {
            using NetworkStream ns = new NetworkStream(this.socket);
            int i;
            List<byte> bs = new List<byte>();
            while((i = ns.ReadByte()) != -1) {
                bs.Add((byte)i);
            }
            return bs.ToArray();
        }

        /// <summary>
        /// Receive a string with a specific length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private string ReceiveString(int length) {
            return Protocol.Decode(this.ReceiveBytes(length));
        }

        /// <summary>
        /// Read 4 bytes as the length of the string and receive the string
        /// </summary>
        /// <returns></returns>
        private string ReceiveString() {
            // The first 4 bytes indicates the length of the following string
            int length = int.Parse(this.ReceiveString(4), NumberStyles.HexNumber);
            return this.ReceiveString(length);
        }
        #endregion Receive

        /// <summary>
        /// Throw error if the reply does not start with 'OKAY'
        /// </summary>
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
