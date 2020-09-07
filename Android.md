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
ImageArray ia = d.Screencap();
// Take screenshot and save as png
d.Screencap("screen.png")
// Save as raw data
ia.Save("screen.raw");
// Save as bitmap
ia.ToBitmap().Save("screen.bmp");
// Load image from raw data
ia = ImageArray.FromFile("screen.raw");
// Load image from bitmap
ia = ImageArray.FromBitmap("screen.bmp");
// PNG works as well
ia = ImageArray.FromBitmap("screen.png");
~~~

### Search for Images
~~~cs
Device d = new Host().Devices.First();
// Get heystack and needle
ImageArray heystack = d.Screencap();
ImageArray needle = ImageArray.FromFile("needle.raw");
// Locate needle inside heystack with 100 tolerance
Point p;
if((p = heystack.Locate(needle, 100)).IsEmpty){
	// not found
}else{
	// found
}
~~~
