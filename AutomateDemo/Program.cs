using Automate;
using System;

namespace AutomateDemo {
    class Program {
        private static Mouse mouse = new Mouse();
        static void Main(string[] args) {
            while(true) {
                Console.ReadLine();
                Console.WriteLine(mouse.GetMousePosition()); 
            }
        }
    }
}
