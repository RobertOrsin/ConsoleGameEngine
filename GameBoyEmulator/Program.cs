using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEngine;
using ProjectDMG;
using static ConsoleGameEngine.NativeMethods;

namespace GameBoyEmulator
{
    class GameBoyEmu : GameConsole
    {
        IntPtr inHandle;
        delegate void MyDelegate();

        ProjectDMG.ProjectDMG dmg;

        ListBox lb_RomFiles;
        List<string> romFiles = new List<string>();
        Button btn_LoadRom;


        public GameBoyEmu()
          : base(160, 144, "GameBoy-Emulator", fontwidth: 4, fontheight: 4)
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

            ConsoleGameEngine.TextWriter.InitTextWriter();
         //   ConsoleGameEngine.TextWriter.LoadFont("fontsheet.txt", 6, 8);
            ConsoleGameEngine.TextWriter.LoadSmallFont("small font.txt", 6, 6);
            dmg = new ProjectDMG.ProjectDMG(this);

            //Load Rom-File-Paths
            foreach (string file in Directory.EnumerateFiles(@"ROMS\", "*.gb"))
                romFiles.Add(Path.GetFileName(file));
            lb_RomFiles = new ListBox(20, 2, 120, 100, romFiles);
            btn_LoadRom = new Button(125, 105, ConsoleGameEngine.TextWriter.GenerateTextSprite("Load",ConsoleGameEngine.TextWriter.Textalignment.Center,1), ConsoleGameEngine.TextWriter.GenerateTextSprite("Load", ConsoleGameEngine.TextWriter.Textalignment.Center, 1, (short)COLOR.BG_WHITE, (short)COLOR.FG_WHITE), method: BtnLoadRomClicked);
            return true;
        }
        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            if (dmg.power_switch)
            {
                //D-Pad
                if (GameConsole.GetKeyState(ConsoleKey.W).Pressed) dmg.joypad.handleKeyDown(20);
                if (GameConsole.GetKeyState(ConsoleKey.A).Pressed) dmg.joypad.handleKeyDown(18);
                if (GameConsole.GetKeyState(ConsoleKey.S).Pressed) dmg.joypad.handleKeyDown(24);
                if (GameConsole.GetKeyState(ConsoleKey.D).Pressed) dmg.joypad.handleKeyDown(17);
                //Button A + B
                if (GameConsole.GetKeyState(ConsoleKey.J).Pressed) dmg.joypad.handleKeyDown(33);
                if (GameConsole.GetKeyState(ConsoleKey.K).Pressed) dmg.joypad.handleKeyDown(34);
                // Start + Select
                if (GameConsole.GetKeyState(ConsoleKey.C).Pressed) dmg.joypad.handleKeyDown(36);
                if (GameConsole.GetKeyState(ConsoleKey.V).Pressed) dmg.joypad.handleKeyDown(40);

                //D-Pad
                if (GameConsole.GetKeyState(ConsoleKey.W).Released) dmg.joypad.handleKeyUp(20);
                if (GameConsole.GetKeyState(ConsoleKey.A).Released) dmg.joypad.handleKeyUp(18);
                if (GameConsole.GetKeyState(ConsoleKey.S).Released) dmg.joypad.handleKeyUp(24);
                if (GameConsole.GetKeyState(ConsoleKey.D).Released) dmg.joypad.handleKeyUp(17);
                //Button A + B
                if (GameConsole.GetKeyState(ConsoleKey.J).Released) dmg.joypad.handleKeyUp(33);
                if (GameConsole.GetKeyState(ConsoleKey.K).Released) dmg.joypad.handleKeyUp(34);
                // Start + Select
                if (GameConsole.GetKeyState(ConsoleKey.C).Released) dmg.joypad.handleKeyUp(36);
                if (GameConsole.GetKeyState(ConsoleKey.V).Released) dmg.joypad.handleKeyUp(40);

                //Power Off DMG
                if (GameConsole.GetKeyState(ConsoleKey.Backspace).Pressed) dmg.POWER_OFF();
            }
            else
            {
                Clear(); 
                DrawSprite(lb_RomFiles.x, lb_RomFiles.y, lb_RomFiles.outputSprite);
                DrawSprite(btn_LoadRom.x, btn_LoadRom.y, btn_LoadRom.outputSprite);


                DrawSprite(0, 115, ConsoleGameEngine.TextWriter.GenerateTextSprite("DPAD - WASD, A/B - J/K",backgroundColor: (short)COLOR.BG_BLACK, foregroundColor: (short)COLOR.FG_WHITE, fontType: ConsoleGameEngine.TextWriter.FontType.small));
                DrawSprite(0, 123, ConsoleGameEngine.TextWriter.GenerateTextSprite("STRT/SLCT - C/V", backgroundColor: (short)COLOR.BG_BLACK, foregroundColor: (short)COLOR.FG_WHITE, fontType: ConsoleGameEngine.TextWriter.FontType.small));
                DrawSprite(0, 131, ConsoleGameEngine.TextWriter.GenerateTextSprite("Close - BCKSPC", backgroundColor: (short)COLOR.BG_BLACK, foregroundColor: (short)COLOR.FG_WHITE, fontType: ConsoleGameEngine.TextWriter.FontType.small));
            }

            //game loop, draw and evaluate inputs
            return true;
        }

        private void ConsoleListener_MouseEvent(MOUSE_EVENT_RECORD r)
        {
            if(btn_LoadRom != null) btn_LoadRom.Update(r);
            if(lb_RomFiles != null) lb_RomFiles.Update(r);

        }

        private bool BtnLoadRomClicked()
        {
            dmg.POWER_ON("ROMS\\" + romFiles[lb_RomFiles.selectedEntry]);

            return true;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

            using (var f = new GameBoyEmu())
                f.Start();
        }
    }
}
