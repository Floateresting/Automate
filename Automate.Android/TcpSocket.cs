using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;

namespace Automate.Android {
    public class TcpSocket : IDisposable {
        #region Properties

        /// <summary>
        /// Gets a value that indicates whether a <see cref="System.Net.Sockets.Socket"/>is connected
        /// </summary>
        public bool Connected => this.Socket.Connected;
        /// <summary>
        /// Gets the amount of data that has been received from the network and is available to be read.
        /// </summary>
        public int Available => this.Socket.Available;

        public Socket Socket { get; set; }
        #endregion Properties


        public TcpSocket(string host, int port) {
            this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.Socket.Connect(host, port);
        }

        /// <summary>
        /// Read the length(4 bytes) and a string
        /// </summary>
        /// <returns></returns>
        internal string GetString() {
            int length = int.Parse(this.ReceiveString(4), NumberStyles.HexNumber);
            return this.ReceiveString(length);
        }

        /// <summary>
        /// Send a string to ADB and check status
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        internal bool Send(string s) {
            this.Socket.Send(Protocol.Encode(s));
            return this.CheckStatus();
        }

        /// <summary>
        /// Read bytes from the socket
        /// </summary>
        /// <param name="length">Number of bytes to read</param>
        /// <returns></returns>
        internal byte[] Receive(int length) {
            byte[] buffer = new byte[length];
            this.Socket.Receive(buffer);
            return buffer;
        }

        internal byte[] ReceiveAll() {
            using NetworkStream ns = new NetworkStream(this.Socket);
            int i;
            List<byte> bs = new List<byte>();
            while((i = ns.ReadByte()) != -1) {
                bs.Add((byte)i);
            }
            return bs.ToArray();
        }

        /// <summary>
        /// Read bytes from the socket and convert to string
        /// </summary>
        /// <param name="length">Number of bytes to read</param>
        /// <returns></returns>
        internal string ReceiveString(int length) {
            return Protocol.Decode(this.Receive(length));
        }

        /// <summary>
        /// Read 4 bytes and check if it's <see cref="Protocol.OKAY"/>
        /// </summary>
        /// <returns></returns>
        internal bool CheckStatus() {
            string status = this.ReceiveString(4);
            if(status != Protocol.OKAY) {
                throw new Exception("Not okay");
            }
            return true;
        }



        public void Dispose() {
            this.Socket.Dispose();
        }
    }
}
