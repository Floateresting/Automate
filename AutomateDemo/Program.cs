using Automate;
using System;

namespace AutomateDemo {
    class Program {
        private static Mouse mouse = new Mouse();
        static void Main(string[] args) {
            while(true) {
                Console.ReadLine();
                mouse.MoveTo(1119, 585);
                Console.WriteLine(mouse.GetMousePosition());
            }
        }
    }
}
