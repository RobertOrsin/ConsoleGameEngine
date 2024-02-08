using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEngine;
using static ConsoleGameEngine.NativeMethods;

namespace SpriteEditor
{
    class SpriteEditor : GameConsole
    {
        IntPtr inHandle;
        delegate void MyDelegate();
 
        int cursorX = 0, cursorY = 0;
        bool leftMousebuttonClicked = false, mouseWheelClicked = false, rightMousebuttonClicked = false;

        short foregroundColor = 0x03, backgroundColor = 0x0F;
        char brush = '▓';

        Sprite sprite = new Sprite(32, 32, '█', COLOR.BG_BLACK);
        Button btnClear, btnSave, btnLoad;
        TextBox tb_Width, tb_Height, tb_SaveName;
        ListBox lb_SavedFiles;

        List<string> sampleEntries = new List<string> { "SuperMario", "CoinAnimation", "Link SNES", "DiddyKongSpriteSheet", "InventoryIcons", "SampleEntrie", "SampleEntrie", "SampleEntrie", "SampleEntrie1", "SampleEntrie2", "SampleEntrie3", "SampleEntrie4", "SampleEntrie5", "SampleEntrie6", "SampleEntrie7", "SampleEntrie8", "SampleEntrie9", "SampleEntrie10", "SampleEntrie11", "SampleEntrie12", "SampleEntrie13", "SampleEntrie14", "SampleEntrie15", "SampleEntrie16", "SampleEntrie17", "SampleEntrie18", "SampleEntrie19" };
        List<string> saveFiles = new List<string>();

        int spriteAreaW = 95, spriteAreaH = 47;
        int spriteCursorX = 0, spriteCursorY = 0;
        int spriteDrawX = 5, spriteDrawY = 10;

        TimeSpan keyInputDelay = new TimeSpan(), keyInputTime = new TimeSpan(0, 0, 0, 0, 120);


        public SpriteEditor()
          : base(140, 70, "Fonts", fontwidth: 12, fontheight: 12)
        { }
        public override bool OnUserCreate()
        {
            ConsoleGameEngine.TextWriter.LoadFont("fontsheet.txt", 7, 9);

            btnClear = new Button(105, 8, "clear / new");
            btnClear.OnButtonClicked(BtnClearClicked);

            tb_Width = new TextBox(119, 7, 6, "Width:");
            tb_Height = new TextBox(129, 7, 6, "Height:", simple: true);

            btnSave = new Button(105, 12, " save ");
            btnSave.OnButtonClicked(BtnSaveClicked);

            tb_SaveName = new TextBox(105, 16, 30, "Save Name:");


            //load savefiles from savefile-folder
            foreach(string file in Directory.EnumerateFiles(@"Savefiles\", "*.txt"))
                saveFiles.Add(Path.GetFileName(file));


            lb_SavedFiles = new ListBox(105, 22, 32, 15, saveFiles, simple: true);

            btnLoad = new Button(129, 38, " load ");
            btnLoad.OnButtonClicked(BtnLoadClicked);

            inHandle = NativeMethods.GetStdHandle(NativeMethods.STD_INPUT_HANDLE);
            uint mode = 0;
            NativeMethods.GetConsoleMode(inHandle, ref mode);
            mode &= ~NativeMethods.ENABLE_QUICK_EDIT_MODE; //disable
            mode |= NativeMethods.ENABLE_WINDOW_INPUT; //enable (if you want)
            mode |= NativeMethods.ENABLE_MOUSE_INPUT; //enable
            NativeMethods.SetConsoleMode(inHandle, mode);

            ConsoleListener.MouseEvent += ConsoleListener_MouseEvent;

            ConsoleListener.Start();

            //Load sprites, setup variables and whatever
            return true;
        }
        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            keyInputDelay += elapsedTime;

            tb_Width.UpdateInput(KeyStates, elapsedTime);
            tb_Height.UpdateInput(KeyStates, elapsedTime);
            tb_SaveName.UpdateInput(KeyStates, elapsedTime);

            //evaluate keyinputs of no textbox is selected
            if(!tb_Height.selected && !tb_Width.selected && !tb_SaveName.selected)
            {
                if (GetKeyState(ConsoleKey.W).Held && keyInputDelay >= keyInputTime)
                {
                    spriteCursorY -= 5;
                    if (spriteCursorY < 0)
                        spriteCursorY = 0;

                    keyInputDelay = new TimeSpan();
                }
                if (GetKeyState(ConsoleKey.A).Held && keyInputDelay >= keyInputTime)
                {
                    spriteCursorX -= 5;
                    if (spriteCursorX < 0)
                        spriteCursorX = 0;

                    keyInputDelay = new TimeSpan();
                }
                if (GetKeyState(ConsoleKey.S).Held && keyInputDelay >= keyInputTime)
                {
                    spriteCursorY += 5;
                    if (spriteCursorY >= sprite.Height - spriteAreaH)
                        spriteCursorY = sprite.Height - 1;

                    keyInputDelay = new TimeSpan();
                }
                if (GetKeyState(ConsoleKey.D).Held && keyInputDelay >= keyInputTime)
                {
                    spriteCursorX += 5;
                    if (spriteCursorX >= sprite.Width - spriteAreaW)
                        spriteCursorX = sprite.Width - 1;

                    keyInputDelay = new TimeSpan();
                }
            }

            EvaluateGUIClick();

            Clear();

            //GUI
            DrawColorPalette(1, 1, "Foregroundcolor");
            DrawColorPalette(40, 1, "Backgroundcolor");
            DrawBrushes(80, 1, "Brushes");
            DrawActiveBrush(100, 1, "Active Brush");

            //DrawArea
            DrawRectangle(3, 8, 100, 50, (short)COLOR.FG_WHITE);
            DrawRectangle(4, 9, 98, 48, (short)COLOR.FG_DARK_GREY);
            //Fill(5, 10, 96, 46, c:'█',  attributes: (short)COLOR.FG_DARK_GREY);

            if (sprite.Width > spriteAreaW || sprite.Height > spriteAreaH)
                DrawPartialSprite(spriteDrawX, spriteDrawY, sprite, spriteCursorX, spriteCursorY, spriteAreaW, spriteAreaH);
            else
                DrawSprite(spriteDrawX, spriteDrawY, sprite);

            Print(3, 7, $"{cursorX};{cursorY}");

            DrawSprite(btnClear.x, btnClear.y, btnClear.outputSprite);
            DrawSprite(btnSave.x, btnSave.y, btnSave.outputSprite);
            DrawSprite(btnLoad.x, btnLoad.y, btnLoad.outputSprite);
            

            DrawSprite(tb_Width.x, tb_Width.y, tb_Width.outputSprite);
            DrawSprite(tb_Height.x, tb_Height.y, tb_Height.outputSprite);
            DrawSprite(tb_SaveName.x, tb_SaveName.y, tb_SaveName.outputSprite);

            DrawSprite(lb_SavedFiles.x, lb_SavedFiles.y, lb_SavedFiles.outputSprite);

            return true;
        }

        private void ConsoleListener_MouseEvent(MOUSE_EVENT_RECORD r)
        {
            btnClear.Update(r);
            btnSave.Update(r);
            btnLoad.Update(r);

            tb_Width.UpdateSelection(r);
            tb_Height.UpdateSelection(r);
            tb_SaveName.UpdateSelection(r);

            lb_SavedFiles.Update(r);

            cursorX = r.dwMousePosition.X;
            cursorY = r.dwMousePosition.Y;

            leftMousebuttonClicked = r.dwButtonState == MOUSE_EVENT_RECORD.FROM_LEFT_1ST_BUTTON_PRESSED;
            mouseWheelClicked = r.dwButtonState == MOUSE_EVENT_RECORD.FROM_LEFT_2ND_BUTTON_PRESSED;
            rightMousebuttonClicked = r.dwButtonState == MOUSE_EVENT_RECORD.RIGHTMOST_BUTTON_PRESSED;
        }


        private void DrawColorPalette(int x, int y, string headline)
        {
            Print(x,y,headline);
            short color = 0x00;
            for(int i = x; i < x + 32; i+=2)
            {
                SetChar(i, y + 1, (char)PIXELS.PIXEL_SOLID, color);
                SetChar(i, y + 2, (char)PIXELS.PIXEL_SOLID, color);
                SetChar(i + 1, y + 1, (char)PIXELS.PIXEL_SOLID, color);
                SetChar(i + 1, y + 2, (char)PIXELS.PIXEL_SOLID, color);

                color++;
            }
        }
        private void DrawBrushes(int x, int y, string headline)
        {
            Print(x,y,headline);

            char[] brushes = new char[4] { '░', '▒', '▓', '█' };
            for(int i = 0; i < 8; i+=2)
            {
                SetChar(x + i, y + 1, brushes[i / 2]);
                SetChar(x + i, y + 2, brushes[i / 2]);
                SetChar(x + i + 1, y + 1, brushes[i / 2]);
                SetChar(x + i + 1, y + 2, brushes[i / 2]);
            }

        }
        private void DrawActiveBrush(int x, int y, string headline)
        {
            Print(x,y,headline);

            short color = (short)(backgroundColor << 4);
            color += foregroundColor;

            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    SetChar(x + i , y + j + 1, brush, color);
                }
            }

        }

        private void EvaluateGUIClick()
        { 
            if(leftMousebuttonClicked)
            {
                //color or brush picking
                if (cursorY == 2 || cursorY == 3)
                {
                    //foreground color
                    if(cursorX >= 1 && cursorX <= 30)
                    {
                        switch(cursorX)
                        {
                            case 1:
                            case 2: foregroundColor = (short)COLOR.FG_BLACK; break;
                            case 3:
                            case 4: foregroundColor = (short)COLOR.FG_DARK_BLUE; break;
                            case 5:
                            case 6: foregroundColor = (short)COLOR.FG_DARK_GREEN; break;
                            case 7:
                            case 8: foregroundColor = (short)COLOR.FG_DARK_CYAN; break;
                            case 9:
                            case 10: foregroundColor = (short)COLOR.FG_DARK_RED; break;
                            case 11:
                            case 12: foregroundColor = (short)COLOR.FG_DARK_MAGENTA; break;
                            case 13:
                            case 14: foregroundColor = (short)COLOR.FG_DARK_YELLOW; break;
                            case 15:
                            case 16: foregroundColor = (short)COLOR.FG_GREY; break;
                            case 17:
                            case 18: foregroundColor = (short)COLOR.FG_DARK_GREY; break;
                            case 19:
                            case 20: foregroundColor = (short)COLOR.FG_BLUE; break;
                            case 21:
                            case 22: foregroundColor = (short)COLOR.FG_GREEN; break;
                            case 23:
                            case 24: foregroundColor = (short)COLOR.FG_CYAN; break;
                            case 25:
                            case 26: foregroundColor = (short)COLOR.FG_RED; break;
                            case 27:
                            case 28: foregroundColor = (short)COLOR.FG_MAGENTA; break;
                            case 29:
                            case 30: foregroundColor = (short)COLOR.FG_YELLOW; break;
                            case 31:
                            case 32: foregroundColor = (short)COLOR.FG_WHITE; break;
                        }
                    }
                    //background color
                    else if(cursorX >= 40 && cursorX <= 69)
                    {
                        switch (cursorX)
                        {
                            case 40:
                            case 41: backgroundColor = (short)COLOR.FG_BLACK; break;
                            case 42:
                            case 43: backgroundColor = (short)COLOR.FG_DARK_BLUE; break;
                            case 44:
                            case 45: backgroundColor = (short)COLOR.FG_DARK_GREEN; break;
                            case 46:
                            case 47: backgroundColor = (short)COLOR.FG_DARK_CYAN; break;
                            case 48:
                            case 49: backgroundColor = (short)COLOR.FG_DARK_RED; break;
                            case 50:
                            case 51: backgroundColor = (short)COLOR.FG_DARK_MAGENTA; break;
                            case 52:
                            case 53: backgroundColor = (short)COLOR.FG_DARK_YELLOW; break;
                            case 54:
                            case 55: backgroundColor = (short)COLOR.FG_GREY; break;
                            case 56:
                            case 57: backgroundColor = (short)COLOR.FG_DARK_GREY; break;
                            case 58:
                            case 59: backgroundColor = (short)COLOR.FG_BLUE; break;
                            case 60:
                            case 61: backgroundColor = (short)COLOR.FG_GREEN; break;
                            case 62:
                            case 63: backgroundColor = (short)COLOR.FG_CYAN; break;
                            case 64:
                            case 65: backgroundColor = (short)COLOR.FG_RED; break;
                            case 66:
                            case 67: backgroundColor = (short)COLOR.FG_MAGENTA; break;
                            case 68:
                            case 69: backgroundColor = (short)COLOR.FG_YELLOW; break;
                            case 70:
                            case 71: backgroundColor = (short)COLOR.FG_WHITE; break;
                        }
                    }
                    //brush
                    else if(cursorX >= 80 && cursorX <= 87)
                    {
                        switch(cursorX)
                        {
                            case 80:
                            case 81: brush = '░'; break;
                            case 82:
                            case 83: brush = '▒'; break;
                            case 84:
                            case 85: brush = '▓'; break;
                            case 86:
                            case 87: brush = '█'; break;
                        }
                    }
                }
                //draw on sprite
                if(cursorX >= 5 && cursorX <= 102 && cursorY >= 10 && cursorY <= 57)
                {
                    if (cursorX - 5 < sprite.Width && cursorY - 10 < sprite.Height)
                    {
                        short color = (short)(backgroundColor << 4);
                        color += foregroundColor;
                        sprite.SetPixel(cursorX - 5 + spriteCursorX, cursorY - 10 + spriteCursorY, brush, color);
                    }
                }
            }
        }

        private bool BtnClearClicked()
        {
            if (tb_Width.content != "" && tb_Height.content != "")
                sprite = new Sprite(Convert.ToInt32(tb_Width.content), Convert.ToInt32(tb_Height.content), '█', COLOR.BG_BLACK);

            spriteCursorX = 0;
            spriteCursorY = 0;

            tb_Width.content = "";
            tb_Height.content = "";

            return true;
        }
        private bool BtnSaveClicked()
        {
            return true;
        }
        private bool BtnLoadClicked()
        {
            sprite = new Sprite("Savefiles\\" + saveFiles[lb_SavedFiles.selectedEntry]);

            return true;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

            using (var f = new SpriteEditor())
                f.Start();
        }
    }
}
