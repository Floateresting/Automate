# Automate

## ScreenCaputre
- Convert from `ScreenCaputre` to `System.Drawing.Bitmap`
- Convert from `System.Drawing.Bitmap` to `ScreenCaputre`
- Locate a small ScreenCaputre(needle) inside a big ScreenCaputre(heystack)
- Locate all needles inside heystack

### Example with Bitmap
(see Android.md for details about `Device`)
~~~cs
Device d = new Host().Devices.First();
// Get heystack and needle
ScreenCaputre heystack = d.Screencap();
// Save heystack image
heystack.ToBitmap().Save("heystack.bmp");
heystack.Save("heystack.raw");
// Load needle image
ScreenCaputre needle = ScreenCaputre.FromFile("needle.raw");
needle = ScreenCaputre.FromBitmap("needle.bmp");
// Locate needle inside heystack with 100 tolerance
Point p0 = heystack.Locate(needle, 100);
if(p0.IsEmpty){
	// not found
}else{
	// found
}
// Locate all 10x20px regions of #39c5bb inside heystack
// with a tolerance of 40
// and set the minumum distance between each found region to 20px
foreach(Point p1 in heystack.LocateColorAll(0x39c5bb, 10, 20, 40, 20)){
	// found
}
~~~

### Example with Template
~~~cs
// Look for one 10x10px region of #39c5bb
// in screen.bmp from (123, 160) to (123 + 1045, 160 + 445)
// with tolerance of 8
Template t0 = new Template(0x39c5bb, 10, (123, 160, 1045, 445), 8);
ScreenCaputre sc0 = ScreenCaputre.FromBitmap("screen.bmp");
Point points = sc0.LocateTemplate(t0);
// Look for all 10x10px region of #39c5bb
// in screen.bmp from (123, 160) to (123 + 1045, 160 + 445)
// with tolerance of 8
// and results will have at least 30px of distance among each other
Template t1 = new Template(0x39c5bb, 10, (123, 160, 1045, 445), 8, 30);
ScreenCaputre sc1 = ScreenCaputre.FromBitmap("screen.bmp");
Point[] points = sc1.LocateTemplateAll(t1).Array();
~~~