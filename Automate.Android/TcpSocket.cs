using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;

namespace Automate.Android {
    public class TcpSocket : IDisposable {
        #region Properties

        /// <summary>
        /// Gets a value that indicates whether a <see cref="System.Net.Sockets.Socket"/>is connected
        /// </summary>
        public bool Connected { get => this.Socket.Connected; }
        /// <summary>
        /// Gets the amount of data that has been received from the network and is available to be read.
        /// </summary>
        public int Available { get => this.Socket.Available; }
        #endregion Properties

        public Socket Socket { get; set; }

        public TcpSocket(string host, int port) {
            this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.Socket.Connect(host, port);
        }

        #region Public

        /// <summary>
        /// Read the length(4 bytes) and a string
        /// </summary>
        /// <returns></returns>
        public string GetString() {
            int length = int.Parse(this.ReceiveString(4), NumberStyles.HexNumber);
            return this.ReceiveString(length);
        }

        /// <summary>
        /// Read 4 bytes and check if it's <see cref="Protocol.OKAY"/>
        /// </summary>
        /// <returns></returns>
        public bool CheckStatus() {
            string status = this.ReceiveString(4);
            if(status != Protocol.OKAY) {
                throw new Exception("Not okay");
            }
            return true;
        }

        /// <summary>
        /// Send a string to ADB and check status
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool Send(string s) {
            this.Socket.Send(Protocol.Encode(s));
            return this.CheckStatus();
        }
        #endregion Public

        #region Internal
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

        /// <summary>
        /// Read bytes from the socket and convert to string
        /// </summary>
        /// <param name="length">Number of bytes to read</param>
        /// <returns></returns>
        internal string ReceiveString(int length) {
            return Protocol.Decode(this.Receive(length));
        }
        #endregion Internal

        public void Dispose() {
            this.Socket.Dispose();
        }
    }
}
