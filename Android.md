# Automate.Android
- Uses Android Debug Brige (adb)

## Automate.Android.Host
- Get a list of connected devices

### Examples
~~~cs
// Connects to the server 127.0.0.1:5057 by default
Host host = new Host();
// Get a list of connected devices
Device d = host.Devices();

Console.WriteLine(d.First().Serial);
~~~


## Automate.Android.Device
- Take screenshot
- Left click

### Examples
~~~cs
Host host = new Host();
Device d = host.Devices.First();
// Take screenshot from device
byte[,][] screenshot = d.Screencap();
// Get {r, g, b, a} values at x=10, y=20
byte[] rgba = byte[10,20];
// Click at x=300, y=100
d.InputTap(300,100);
~~~

