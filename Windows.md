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
