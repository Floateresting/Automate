# Automate
Control mouse, search image on screen and resize third-party application


### Usage
Build the library, and add reference

### Automate.Mouse
Features
  - Get mouse position
  - Set mouse position
  - Perform clicks (only left clicks, but can do right clicks by changing source code)
  - Perform horizontal/vertical drags

###### Examples
~~~cs
using System.Drawing;
using Automate;

class Program {
    private readonly static Mouse mouse = new Mouse();

    static void Main(string[] args){
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
    }
}

~~~

### Bitmap Extensions
Features
  - Find a bitmap with tolerance (returns the first match)
  - Find a region of a solid color with tolerance (returns the first match)
  - Find a bitmap with tolerance (returns all matches)
  - Find a region of a solid color with tolerance (returns all matches)

##### Examples
~~~cs
class Program {
    static void Main(string[] args) {
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
    }
}

~~~

### Automate.Application
Features
  - Set focus to a third-party application
  - Resize and/or move the application's window

##### Examples
~~~cs
class Program {
    // Link to VisualStudio (VisualStudio's process name is 'devenv')
    // process name's can be found in 'Task Manager -> Details'
    private static readonly Application application = new Application("devenv");

    static void Main(string[] args) {
        // Bring the window to front (or set focus to this window)
        application.SetFocus();
        
        // Move the window to top-left corner and resize it to 1280x720
        application.ResizeAndMove(0, 0, 1280, 720);
        application.ResizeAndMove(new Point(0, 0), new Size(1280, 720));
    }
}
~~~
