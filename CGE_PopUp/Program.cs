using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEngine;
using static ConsoleGameEngine.NativeMethods;

namespace CGE_PopUp
{
    class CGE_PopUp : GameConsole
    {
        IntPtr inHandle;
        delegate void MyDelegate();

        Button button;
        PopUp popUp;
        Sprite popUpSprite;
        PopUpState popUpState;

        public CGE_PopUp()
          : base(200, 120, "Fonts", fontwidth: 4, fontheight: 4)
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

            TextWriter.LoadFont("fontsheet.txt", 7, 9);

            button = new Button(0, 0, TextWriter.GenerateTextSprite("open PopUp", TextWriter.Textalignment.Left, 1));
            button.OnButtonClicked(ButtonClicked);

            popUp = new PopUp(40, 40, "Are you sure?", out popUpSprite);

            ConsoleListener.MouseEvent += ConsoleListener_MouseEvent;

            return true;
        }
        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            Clear();

            DrawSprite(0, 0, button.outputSprite);

            if (popUp.visible)
                DrawSprite(40, 40, popUpSprite);

            if(popUpState != PopUpState.none)
            {
                popUp.visible = false;
            }

            return true;
        }

        private void ConsoleListener_MouseEvent(MOUSE_EVENT_RECORD r)
        {
            popUpState = popUp.Update(r);
            button.Update(r);
        }

        private bool ButtonClicked()
        {
            popUp.visible = true;
            return true;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

            using (var f = new CGE_PopUp())
                f.Start();
        }
    }
}
