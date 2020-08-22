using System.Security.Cryptography;

namespace Automate.Android {
    public class Device {
        public string Serial { get; set; }
        private readonly Client c;
        public Device(Client c, string serial) {
            this.c = c;
            this.Serial = serial;
        }

        public void Screencap() {
            using TcpSocket ts = this.CreateSocket();
            ts.Send("shell:/system/bin/screencap -p");
        }

        private void Transport(TcpSocket ts) {
            ts.Send($"host:transport:{this.Serial}");
        }

        private TcpSocket CreateSocket(bool transport = true) {
            TcpSocket ts = this.c.CreateSocket();
            if(transport) {
                this.Transport(ts);
            }
            return ts;
        }
    }
}
