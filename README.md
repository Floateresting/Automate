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

### Automate.Window
Features
  - Find a image on screen with optional tolerance (returns the first match)
  - Find a image on screen with optional tolerance (returns the all matches)
  - Get actual tolerance of the image found on screen
  - Save a screenshot to a bitmap file (.bmp)

##### Examples
~~~cs
class Program {
	// By default it will use the entire screen
    private readonly static Window window1 = new Window();
    // Only 1280x720 from the top-left corner
    private readonly static Window window2 = new Window(0, 0, 1280, 720);
    // 1280x720 from (100, 100)
    private readonly static Window window3 = new Window(new Point(100, 100), new Size(1280, 720));

    static void Main(string[] args) {
    	// Search for the exact image (no tolerance)
        bool sucess1 = window1.LocateOnScreen("template.bmp", out Point result1);
        // Search for the image with tolerance 30, from (500, 500) to (600, 600) of the screen
        bool sucess2 = window2.LocateOnScreen("template.bmp", out Point result2, 30, new Point(500, 500), new Size(100, 100));
        // Get the actual tolerance found on screen with estimated tolenance of 60
        bool sucess3 = window3.LocateOnScreen("template.bmp", out _, out double actualTolerance, 60);
        // Search for all matches on the screen, where each match's distance is larger than 30 px 
        bool sucess4 = window1.LocateAllOnScreen("template.bmp", 30, out List<Point> results, 40);

        // Save screenshot to 'Capture.bmp'
        window1.SaveScreenshot("Capture.bmp");
        // Save screenshot to 'Capture.bmp', from (200, 200) to (600, 600) of the screen
        window2.SaveScreenshot("Capture", new Point(200, 200), new Size(400, 400));
        // Save screenshot to 'Screenshot.bmp'
        window3.SaveScreenshot();
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
