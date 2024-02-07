using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEngine;
using static ConsoleGameEngine.NativeMethods;

namespace CGE_Textbox
{
    class CGE_Textbox : GameConsole
    {
        TextBox tb_Input1, tb_Input2;

        IntPtr inHandle;
        delegate void MyDelegate();


        public CGE_Textbox()
          : base(200, 120, "Fonts", fontwidth: 4, fontheight: 4)
        { }
        public override bool OnUserCreate()
        {
            TextWriter.LoadFont("fontsheet.txt", 7, 9);

            tb_Input1 = new TextBox(0, 15, 10, "Input 1");
            tb_Input2 = new TextBox(20, 15, 10, "Input 2", simple: false);

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
            tb_Input1.UpdateInput(KeyStates, elapsedTime);
            tb_Input2.UpdateInput(KeyStates, elapsedTime);

            DrawSprite(tb_Input1.x, tb_Input1.y, tb_Input1.outputSprite);
            DrawSprite(tb_Input2.x, tb_Input2.y, tb_Input2.outputSprite);

            DrawSprite(0, 40, TextWriter.GenerateTextSprite($"Your inputs are {tb_Input1.content}\nand {tb_Input2.content}\n...weird...", TextWriter.Textalignment.Left, 1)); ;

            return true;
        }

        private void ConsoleListener_MouseEvent(MOUSE_EVENT_RECORD r)
        {
            tb_Input1.UpdateSelection(r);
            tb_Input2.UpdateSelection(r);

        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

            using (var f = new CGE_Textbox())
                f.Start();
        }
    }
}
