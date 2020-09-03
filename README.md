# Automate

## ImageArray
- Convert from `ImageArray` to `System.Drawing.Bitmap`
- Convert from `System.Drawing.Bitmap` to `ImageArray`
- Locate a small ImageArray(needle) inside a big ImageArray(heystack)
- Locate all needles inside heystack

### Examples
~~~cs
// The heystack can be an entire screenshot
Bitmap heystack = new Bitmap("/path/to/heystack.bmp");
Bitmap needle = new Bitmap("/path/to/needle.bmp");
// Search for a 10x10px solid color of #161616 (r22, g22, b22) with tolerance of 5
Point p0 = heystack.LocateColor(new byte[] { 22, 22, 22 }, new Size(10, 10), 5);
// Search for needle.bmp with tolerance of 10
Point p1 = heystack.LocateBitmap(needle, 10);
// Use IsEmpty to check if found
if(p1.IsEmpty){
    // do stuff
}
// Search for all 10x10px solid color of #161616 with:
//  - tolerance of 5
//  - minimum 10px of distance between each 10x10 found region
foreach(Point p in heystack.LocateColorAll(new byte[] { 22, 22, 22 }, new Size(10, 10), 5, 10)){
    // do stuff with each found point
}
~~~


## Byte Array Extensions
- Convert byte[x,y][] to Bitmap

### Examples
~~~cs
Host host = new Host();
Device d = host.Devices.First();
byte[,][] screenshot = d.Screencap();
// Convert raw data from Screencap() to Bitmap object
Bitmap b = screenshot.ToBitmap();
b.Save("screen.bmp");
~~~

