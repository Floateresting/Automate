using System.Text;

namespace Automate.Android {
    public class Protocol {
        public const string OKAY = "OKAY";
        public const string FAIL = "FAIL";
        public const string STAT = "STAT";
        public const string LIST = "LIST";
        public const string DENT = "DENT";
        public const string RECV = "RECV";
        public const string DATA = "DATA";
        public const string DONE = "DONE";
        public const string SEND = "SEND";
        public const string QUIT = "QUIT";

        /// <summary>
        /// Decode bytes received from ADB
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string Decode(byte[] b) {
            return Encoding.UTF8.GetString(b);
        }

        /// <summary>
        /// Encode a string to bytes for ADB
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] Encode(string s) {
            // Length in hex + message
            return Encoding.UTF8.GetBytes(s.Length.ToString("x4") + s);
        }
    }
}
