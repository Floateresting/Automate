using System;
using System.Collections.Generic;
using System.Text;

namespace Automate.Android.DebugBridge {
    public enum DeviceState {
        Offline,
        BootLoader,
        Online,
        Host,
        Recovory,
        NoPermissions,
        Sideload,
        Unauthorized,
        Authorizing,
        Unknown
    }
}
