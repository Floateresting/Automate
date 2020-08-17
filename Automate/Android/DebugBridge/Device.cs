using System;
using System.Text.RegularExpressions;

namespace Automate.Android.DebugBridge {
    public class Device {
        #region Regex

        /// <summary>
        /// Used to parse the information returned by adb
        /// </summary>
        internal const string RegexString = @"^(?<serial>[a-zA-Z0-9_-]+(?:\s?[\.a-zA-Z0-9_-]+)?(?:\:\d{1,})?)\s+(?<state>device|connecting|offline|unknown|bootloader|recovery|download|authorizing|unauthorized|host|no permissions)(?<message>.*?)(\s+usb:(?<usb>[^:]+))?(?:\s+product:(?<product>[^:]+))?(\s+model\:(?<model>[\S]+))?(\s+device\:(?<device>[\S]+))?(\s+features:(?<features>[^:]+))?(\s+transport_id:(?<transport_id>[^:]+))?$";
        internal static readonly Regex Regex = new Regex(Device.RegexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        #endregion Regex

        #region Properties

        public string Serial { get; set; }
        public DeviceState State { get; set; }
        public string Model { get; set; }
        public string Product { get; set; }
        public string Name { get; set; }
        public string Features { get; set; }
        public string Usb { get; set; }
        public string TransportId { get; set; }
        public string Message { get; set; }
        #endregion Properties

        public static Device FromString(string s) {
            Match m = Device.Regex.Match(s);
            if(m.Success) {
                return new Device {
                    Serial = m.Groups["serial"].Value,
                    State = m.Groups["state"].Value.ToDeviceState(),
                    Model = m.Groups["model"].Value,
                    Product = m.Groups["product"].Value,
                    Name = m.Groups["device"].Value,
                    Features = m.Groups["features"].Value,
                    Usb = m.Groups["usb"].Value,
                    TransportId = m.Groups["transport_id"].Value,
                    Message = m.Groups["message"].Value
                };
            } else {
                throw new Exception($"Invalid device data: {s}");
            }
        }
    }
}
