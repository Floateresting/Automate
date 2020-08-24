# Automate

## Bitmap Extensions
- Find a bitmap with tolerance (returns the first match)
- Find a region of a solid color with tolerance (returns the first match)
- Find a bitmap with tolerance (returns all matches)
- Find a region of a solid color with tolerance (returns all matches)

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
Client client = new Client();
Device d = client.Devices.First();
byte[,][] screenshot = d.Screencap();
// Convert raw data from Screencap() to Bitmap object
Bitmap b = screenshot.ToBitmap();
b.Save("screen.bmp");
~~~

# Automate.Android
- Uses Android Debug Brige (adb)

## Automate.Android.Client
- Get a list of connected devices

### Examples
~~~cs
// Connects to the server 127.0.0.1:5057 by default
Client client = new Client();
// Get a list of connected devices
Device d = client.Devices();

Console.WriteLine(d.First().Serial);
~~~


## Automate.Android.Device
- Take screenshot
- Left click

### Examples
~~~cs
Client client = new Client();
Device d = client.Devices.First();
// Take screenshot from device
byte[,][] screenshot = d.Screencap();
// Get {r, g, b, a} values at x=10, y=20
byte[] rgba = byte[10,20];
// Click at x=300, y=100
d.InputTap(300,100);
~~~

# Automate.Windows

## Automate.Windows.Mouse
- Get mouse position
- Set mouse position
- Perform clicks (only left clicks, but can do right clicks by changing source code)
- Perform horizontal/vertical drags

### Examples
~~~cs
Mouse mouse = new Mouse();
// Move to cursor to (500, 500)
mouse.MoveTo(new Point(500, 500));
mouse.MoveTo(500, 500);

// Left click at current cursor's position
mouse.Click();

// Left click at (400, 400)
mouse.ClickAt(400, 400);
mouse.ClickAt(new Point(400,400));

// From (300, 300), move 200 pixels towards right at 2 pixel/ms
mouse.DragH(new Point(300, 300), 200, 2);
// From (200, 200), move 100 pixels upwards at 3 pixel/ms
mouse.DragV(new Point(200, 200), -100, 3);

// Get current cursor's position
Point cursorPos = mouse.GetMousePosition();
~~~


## Automate.Windows.Application
- Set focus to a third-party application
- Resize and/or move the application's window

### Examples
~~~cs
// Link to VisualStudio (VisualStudio's process name is 'devenv')
// process name's can be found in 'Task Manager -> Details'
Application application = new Application("devenv");
// Bring the window to front (or set focus to this window)
application.SetFocus();

// Move the window to top-left corner and resize it to 1280x720
application.ResizeAndMove(0, 0, 1280, 720);
application.ResizeAndMove(new Point(0, 0), new Size(1280, 720));
~~~
