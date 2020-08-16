using System;
using System.Collections.Generic;
using System.Text;

namespace Automate.Android.DebugBridge {
    internal static class Extensions {
        internal static byte[] ToAdbBytes(this string request, Encoding encoding) {
            return encoding.GetBytes(request.Length.ToString("x4") + request);
        }
        internal static DeviceState ToDeviceState(this string s) {
            switch(s) {
                case "device":
                    return DeviceState.Online;
                case "no permissions":
                    return DeviceState.NoPermissions;
                default:
                    if(Enum.TryParse<DeviceState>(s, true, out var ds)) {
                        return ds;
                    } else {
                        return DeviceState.Unknown;
                    }
            }
        }
    }
}
