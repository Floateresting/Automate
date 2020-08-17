using System;
using System.Text;

namespace Automate.Android.DebugBridge {
    internal static class StringExtensions {
        /// <summary>
        /// Convert to bytes for TCP requests
        /// </summary>
        /// <param name="request"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        internal static byte[] ToAdbBytes(this string request, Encoding encoding) {
            return encoding.GetBytes(request.Length.ToString("x4") + request);
        }

        /// <summary>
        /// Convert to <see cref="DeviceState"/> when creating a <see cref="Device"/> object
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        internal static DeviceState ToDeviceState(this string s) {
            switch(s) {
                case "device":
                    return DeviceState.Online;
                case "no permissions":
                    return DeviceState.NoPermissions;
                default:
                    if(Enum.TryParse<DeviceState>(s, true, out DeviceState ds)) {
                        return ds;
                    } else {
                        return DeviceState.Unknown;
                    }
            }
        }
    }
}
