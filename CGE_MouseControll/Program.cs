using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEngine;

namespace CGE_MouseControll
{
    class MouseControll : GameConsole
    {

        IntPtr inHandle;


        public MouseControll()
          : base(200, 120, "MouseControll", fontwidth: 4, fontheight: 4)
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

            ConsoleListener.Start();

            return true;
        }

        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            Clear();

            SetChar(ConsoleListener.record[0].MouseEvent.dwMousePosition.X, ConsoleListener.record[0].MouseEvent.dwMousePosition.Y, 'X');




            return true;
        }
    }


    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

            using (var f = new MouseControll())
                f.Start();
        }
    }
}
