using System;
using System.Collections.Generic;
using System.Text;

namespace Automate.Android.DebugBridge {
    internal static class Extensions {
        internal static byte[] ToAdbBytes(this string request, Encoding encoding) {
            return encoding.GetBytes(request.Length.ToString("x4") + request);
        }
    }
}
