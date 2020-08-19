using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Automate.Android.DebugBridge {
    public static class ClientExtensions {
        /// <summary>
        /// Get a list of available devices
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async Task<List<Device>> GetDevicesAsync(this Client client) {
            string[] devices = (await client.GetStringAsync("host:devices-l"))
                .Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return devices.Select(d => Device.FromString(d)).ToList();
        }

        /// <summary>
        /// Ask to switch the connection to a device/emulator
        /// </summary>
        /// <param name="client"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static async Task SetDeviceAsync(this Client client, Device d) {
            Debug.Assert(d != null);
            await client.WriteAsync($"host:transport:{d.Serial}");
        }
    }
}
