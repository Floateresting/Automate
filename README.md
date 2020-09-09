# Automate

## ImageArray
- Convert from `ImageArray` to `System.Drawing.Bitmap`
- Convert from `System.Drawing.Bitmap` to `ImageArray`
- Locate a small ImageArray(needle) inside a big ImageArray(heystack)
- Locate all needles inside heystack

### Examples
(see Android.md for details about `Device`)
~~~cs
Device d = new Host().Devices.First();
// Get heystack and needle
ImageArray heystack = d.Screencap();
// Save heystack image
heystack.ToBitmap().Save("heystack.bmp");
heystack.Save("heystack.raw");
// Load needle image
ImageArray needle = ImageArray.FromFile("needle.raw");
needle = ImageArray.FromBitmap("needle.bmp");
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