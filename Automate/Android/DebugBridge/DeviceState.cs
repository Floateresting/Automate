namespace Automate.Android.DebugBridge {
    /// <summary>
    /// State of an Androind device connected to ADB
    /// </summary>
    public enum DeviceState {
        /// <summary>
        /// Not connected to adb or not responding
        /// </summary>
        Offline,
        /// <summary>
        /// Bootloader mode
        /// </summary>
        BootLoader,
        /// <summary>
        /// Nomal state
        /// </summary>
        Online,
        /// <summary>
        /// The device is the adb host
        /// </summary>
        Host,
        /// <summary>
        /// Recovery Mode
        /// </summary>
        Recovery,
        /// <summary>
        /// Not enough permissions to communicate with the device
        /// </summary>
        NoPermissions,
        /// <summary>
        /// Sideload mode
        /// </summary>
        Sideload,
        /// <summary>
        /// Connected, but the device refuses to authorize adb for debugging
        /// </summary>
        Unauthorized,
        /// <summary>
        /// Connected, but adb is asking for authorization
        /// </summary>
        Authorizing,
        /// <summary>
        /// Unrecognized state
        /// </summary>
        Unknown
    }
}
