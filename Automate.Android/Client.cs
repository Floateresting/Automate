using Microsoft.Win32.SafeHandles;
using System;
using System.Net.Sockets;

namespace Automate.Android {
    public class Client{
        public string Host { get; set; }
        public int Port { get; set; }

        public Client(string hostname = "127.0.0.1", int port = 5037) {
            this.Host = hostname;
            this.Port = port;
        }

        /// <summary>
        /// Send a command to ADB
        /// </summary>
        /// <param name="s"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        public string Send(string s, bool receive = true) {
            using TcpSocket tc = new TcpSocket(this.Host, this.Port);
            tc.Send(s);
            if(receive) {
                string result = tc.GetString();
                return result;
            } else {
                tc.CheckStatus();
                return null;
            }
        }

        /// <summary>
        ///  Get a list of devices available for communication
        /// </summary>
        /// <returns></returns>
        public string Devices() {
            return this.Send("host:devices");
        }
    }
}
