using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEngine;
using static ConsoleGameEngine.NativeMethods;

namespace CGE_UIElements_Simple
{

    class CGE_UIElements_Simple : GameConsole
    {
        //Mouse-Controll
        IntPtr inHandle;
        delegate void MyDelegate();

        //UI-Elements
        TextBlock yourTextBlock;
        TextBox yourTextBox;
        Button yourButton;
        ListBox yourListBox;
        ComboBox yourComboBox;

        List<string> entries = new List<string>() { "Entry 1", "Entry 2", "Entry 3", "Entry 4", "Entry 5", "Entry 6" };



        public CGE_UIElements_Simple()
          : base(80, 50, "Fonts", fontwidth: 10, fontheight: 10)
        { }
        public override bool OnUserCreate()
        {
            yourTextBlock = new TextBlock(2, 2, 15, "i display text!", content:"Like this one");
            yourTextBox = new TextBox(2, 6, 15, "i take inputs");
            yourButton = new Button(20, 2, "Click me", method: OnButtonClick);
            yourListBox = new ListBox(20, 6, 15, 10, entries, simple: true);
            yourComboBox = new ComboBox(40, 2, 15, 20, "I take selections", entries, entriesToShow: 8);

            //Mouse-Input-Init
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

            yourTextBox.UpdateInput(KeyStates, elapsedTime);

            DrawSprite(yourTextBlock.x, yourTextBlock.y, yourTextBlock.outputSprite);
            DrawSprite(yourTextBox.x, yourTextBox.y, yourTextBox.outputSprite);
            DrawSprite(yourButton.x, yourButton.y, yourButton.outputSprite);
            DrawSprite(yourListBox.x, yourListBox.y, yourListBox.outputSprite);
            DrawSprite(yourComboBox.x, yourComboBox.y, yourComboBox.outputSprite);

            return true;
        }
        private void ConsoleListener_MouseEvent(MOUSE_EVENT_RECORD r)
        {
            yourTextBox.UpdateSelection(r);
            yourButton.Update(r);
            yourListBox.Update(r);
            yourComboBox.UpdateMouseInput(r);
        }

        private bool OnButtonClick()
        {
            return true;
        }
    }



    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

            using (var f = new CGE_UIElements_Simple())
                f.Start();
        }
    }
}
