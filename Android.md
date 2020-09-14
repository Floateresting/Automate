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
- Search for images

### Take Screenshots
~~~cs
Device d = new Host().Devices.First();
// Take screenshot from device
ScreenCapture sc = d.Screencap();
// Take screenshot from device and save as png
d.Screencap("screen.png")
// Save as raw data
sc.Save("screen.raw");
// Save as bitmap
sc.ToBitmap().Save("screen.bmp");
// Load image from raw data
sc = ScreenCapture.FromFile("screen.raw");
// Load image from bitmap
sc = ScreenCapture.FromBitmap("screen.bmp");
// PNG works as well
sc = ScreenCapture.FromBitmap("screen.png");
~~~

### Search for Images
(see README.md)