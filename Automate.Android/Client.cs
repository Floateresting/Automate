using Microsoft.Win32.SafeHandles;
using System;
using System.Net.Sockets;

namespace Automate.Android {
    public class Client{
        public string Hostname { get; set; }
        public int Port { get; set; }

        public Client(string hostname = "127.0.0.1", int port = 5037) {
            this.Hostname = hostname;
            this.Port = port;
        }


        public string Send(string s, bool receive = true) {
            using TcpSocket tc = new TcpSocket(this.Hostname, this.Port);
            tc.Send(s);
            if(receive) {
                string result = tc.GetString();
                return result;
            } else {
                tc.CheckStatus();
                return null;
            }
        }

        public string Devices() {
            return this.Send("host:devices");
        }
    }
}
