using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEngine;
using static ConsoleGameEngine.NativeMethods;

namespace CGE_ComboBox
{
    class CGE_ComboBox : GameConsole
    {
        IntPtr inHandle;
        delegate void MyDelegate();

        int cursorX = 0, cursorY = 0;
        bool leftMousebuttonClicked = false, mouseWheelClicked = false, rightMousebuttonClicked = false;


        ComboBox cb_YourComboBox = new ComboBox(10, 10, 20, 30, "Combobox", new List<string> { "Entry 1", "Entry 2", "Entry 3", "Entry 4", "Entry 5" });


        public CGE_ComboBox()
          : base(80, 40, "Fonts", fontwidth: 10, fontheight: 10)
        { }
        public override bool OnUserCreate()
        {
            inHandle = NativeMethods.GetStdHandle(NativeMethods.STD_INPUT_HANDLE);
            uint mode = 0;
            NativeMethods.GetConsoleMode(inHandle, ref mode);
            mode &= ~NativeMethods.ENABLE_QUICK_EDIT_MODE; //disable
            mode |= NativeMethods.ENABLE_WINDOW_INPUT; //enable (if you want)
            mode |= NativeMethods.ENABLE_MOUSE_INPUT; //enable
            NativeMethods.SetConsoleMode(inHandle, mode);

            ConsoleListener.MouseEvent += ConsoleListener_MouseEvent;

            ConsoleListener.Start();

            return true;
        }
        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            Clear();
            DrawSprite(cb_YourComboBox.x, cb_YourComboBox.y, cb_YourComboBox.outputSprite);
            return true;
        }

        private void ConsoleListener_MouseEvent(MOUSE_EVENT_RECORD r)
        {
            cb_YourComboBox.UpdateMouseInput(r);
        }
    }




    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

            using (var f = new CGE_ComboBox())
                f.Start();

        }
    }
}
