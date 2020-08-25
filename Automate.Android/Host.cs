using System.Linq;

namespace Automate.Android {
    public class Host {
        public string Hostname { get; set; }
        public int Port { get; set; }

        public Host(string hostname = "127.0.0.1", int port = 5037) {
            this.Hostname = hostname;
            this.Port = port;
        }

        /// <summary>
        ///  Get a list of devices available for communication
        /// </summary>
        /// <returns></returns>
        public Device[] Devices() {
            using TcpSocket tc = new TcpSocket(this.Hostname, this.Port);
            return tc.GetString("host:devices")
                .Split("\n")
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => new Device(this, l.Split()[0]))
                .ToArray();
        }

        internal TcpSocket CreateConnection(string serial) {
            TcpSocket ts = new TcpSocket(this.Hostname, this.Port);
            // Redirect all commands to this device
            ts.Send($"host:transport:{serial}");
            return ts;
        }
    }
}
