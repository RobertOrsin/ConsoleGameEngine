using System;
using System.Collections.Generic;
using System.Data;
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
        MOUSE_EVENT_RECORD oldMouseState;


        int cursorX = 0, cursorY = 0;
        bool leftMousebuttonClicked = false, leftMousebuttonHeld = false, leftMouseButtonReleased = false, mouseWheelClicked = false, rightMousebuttonClicked = false;

        short foregroundColor = 0x03, backgroundColor = 0x0F;
        char brush = '▓';

        Sprite sprite = new Sprite(32, 32, '█', COLOR.BG_BLACK);
        Button btnClear, btnSave, btnLoad, btnColorPicker, btnMark, btnCopy, btnAbortMarkAndCopy, btnConfirmMarkAndCopy;
        TextBox tb_Width, tb_Height, tb_SaveName;
        ListBox lb_SavedFiles;
        AnimationPreview animationPreview;

        bool colorPickerActive = false, markingActive = false;

        List<string> sampleEntries = new List<string> { "SuperMario", "CoinAnimation", "Link SNES", "DiddyKongSpriteSheet", "InventoryIcons", "SampleEntrie", "SampleEntrie", "SampleEntrie", "SampleEntrie1", "SampleEntrie2", "SampleEntrie3", "SampleEntrie4", "SampleEntrie5", "SampleEntrie6", "SampleEntrie7", "SampleEntrie8", "SampleEntrie9", "SampleEntrie10", "SampleEntrie11", "SampleEntrie12", "SampleEntrie13", "SampleEntrie14", "SampleEntrie15", "SampleEntrie16", "SampleEntrie17", "SampleEntrie18", "SampleEntrie19" };
        List<string> saveFiles = new List<string>();

        int spriteAreaW = 95, spriteAreaH = 47;
        int spriteCursorX = 0, spriteCursorY = 0;
        int spriteDrawX = 5, spriteDrawY = 10;

        bool marking_visible = false, markingDraging = false;
        int markingStartX, markingStartY, markingEndX, markingEndY;
        int markingSpriteX, markingSpriteY;
        Sprite markingSprite;

        TimeSpan keyInputDelay = new TimeSpan(), keyInputTime = new TimeSpan(0, 0, 0, 0, 120);


        public SpriteEditor()
          : base(140, 70, "Fonts", fontwidth: 12, fontheight: 12)
        { }
        public override bool OnUserCreate()
        {
            ConsoleGameEngine.TextWriter.LoadFont("fontsheet.txt", 7, 9);

            btnClear = new Button(105, 8, "clear / new", method: BtnClearClicked);

            tb_Width = new TextBox(119, 7, 6, "Width:");
            tb_Height = new TextBox(129, 7, 6, "Height:", simple: true);

            btnSave = new Button(105, 12, " save ", method: BtnSaveClicked);

            tb_SaveName = new TextBox(105, 16, 30, "Save Name:");

            animationPreview = new AnimationPreview(105, 43);

            btnColorPicker = new Button(115, 2, "pick color", method: BtnColorPickerClicked);

            btnMark = new Button(3, 61, " Mark ", method:BtnMarkClicked);
            btnCopy = new Button(12, 61, " Copy ", method: BtnCopyClicked);
            btnAbortMarkAndCopy = new Button(21, 61, "Abort", method:BtnAbortClicked);
            btnConfirmMarkAndCopy = new Button(30, 61, " Set ", method: BtnConfirmClicked);

            //load savefiles from savefile-folder
            foreach (string file in Directory.EnumerateFiles(@"Savefiles\", "*.txt"))
                saveFiles.Add(Path.GetFileName(file));

            lb_SavedFiles = new ListBox(105, 22, 32, 15, saveFiles, simple: true);

            btnLoad = new Button(129, 38, " load ", method: BtnLoadClicked);

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

            if (ApplicationIsActivated())
            {
                tb_Width.UpdateInput(KeyStates, elapsedTime);
                tb_Height.UpdateInput(KeyStates, elapsedTime);
                tb_SaveName.UpdateInput(KeyStates, elapsedTime);
                animationPreview.UpdateKeyInput(KeyStates, elapsedTime, sprite);

                //evaluate keyinputs of no textbox is selected
                if (!tb_Height.selected && !tb_Width.selected && !tb_SaveName.selected)
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

            if (sprite.Width > spriteAreaW || sprite.Height > spriteAreaH)
                DrawPartialSprite(spriteDrawX, spriteDrawY, sprite, spriteCursorX, spriteCursorY, spriteAreaW, spriteAreaH);
            else
                DrawSprite(spriteDrawX, spriteDrawY, sprite);

            if(marking_visible)
            {
                markingSprite = sprite.ReturnPartialSpriteInverted(markingStartX, markingStartY, markingEndX - markingStartX + 1, markingEndY - markingStartY + 1);
                DrawSprite(markingSpriteX, markingSpriteY, markingSprite);
            }

            Print(3, 7, $"{cursorX - spriteDrawX};{cursorY - spriteDrawY}");

            DrawSprite(btnClear.x, btnClear.y, btnClear.outputSprite);
            DrawSprite(btnSave.x, btnSave.y, btnSave.outputSprite);
            DrawSprite(btnLoad.x, btnLoad.y, btnLoad.outputSprite);

            if(colorPickerActive)
                DrawASCIIRectangle(btnColorPicker.x - 1, btnColorPicker.y - 1, btnColorPicker.width + 2, btnColorPicker.height + 2, foreground: (short)COLOR.FG_RED);
            DrawSprite(btnColorPicker.x, btnColorPicker.y, btnColorPicker.outputSprite);
            
            DrawSprite(tb_Width.x, tb_Width.y, tb_Width.outputSprite);
            DrawSprite(tb_Height.x, tb_Height.y, tb_Height.outputSprite);
            DrawSprite(tb_SaveName.x, tb_SaveName.y, tb_SaveName.outputSprite);

            DrawSprite(lb_SavedFiles.x, lb_SavedFiles.y, lb_SavedFiles.outputSprite);

            DrawSprite(animationPreview.x, animationPreview.y, animationPreview.outputSprite);

            DrawSprite(btnMark.x, btnMark.y, btnMark.outputSprite);
            if (markingActive)
                DrawASCIIRectangle(btnMark.x - 1, btnMark.y - 1, btnMark.width + 2, btnMark.height + 2, foreground: (short)COLOR.FG_RED);
            DrawSprite(btnCopy.x, btnCopy.y, btnCopy.outputSprite);
            DrawSprite(btnAbortMarkAndCopy.x, btnAbortMarkAndCopy.y, btnAbortMarkAndCopy.outputSprite);
            DrawSprite(btnConfirmMarkAndCopy.x, btnConfirmMarkAndCopy.y, btnConfirmMarkAndCopy.outputSprite);


            Print(0, Height - 1, $"marking active:{markingActive}; draging:{markingDraging}");

            return true;
        }

        #region INPUTS
        private void ConsoleListener_MouseEvent(MOUSE_EVENT_RECORD r)
        {
            btnClear.Update(r);
            btnSave.Update(r);
            btnLoad.Update(r);
            btnColorPicker.Update(r);

            btnMark.Update(r);
            btnCopy.Update(r);
            btnAbortMarkAndCopy.Update(r);
            btnConfirmMarkAndCopy.Update(r);


            tb_Width.UpdateSelection(r);
            tb_Height.UpdateSelection(r);
            tb_SaveName.UpdateSelection(r);

            lb_SavedFiles.Update(r);

            animationPreview.UpdateMouseInput(r);

            cursorX = r.dwMousePosition.X;
            cursorY = r.dwMousePosition.Y;

            leftMousebuttonClicked = false;
            leftMouseButtonReleased = false;

            if(r.dwButtonState != oldMouseState.dwButtonState)
            {
                if (r.dwButtonState == MOUSE_EVENT_RECORD.FROM_LEFT_1ST_BUTTON_PRESSED)
                {
                    leftMousebuttonClicked = !leftMousebuttonHeld;
                    leftMousebuttonHeld = true;
                }
                else
                {
                    leftMouseButtonReleased = true;
                    leftMousebuttonHeld = false;
                }
            }
            oldMouseState = r;

            mouseWheelClicked = r.dwButtonState == MOUSE_EVENT_RECORD.FROM_LEFT_2ND_BUTTON_PRESSED;
            rightMousebuttonClicked = r.dwButtonState == MOUSE_EVENT_RECORD.RIGHTMOST_BUTTON_PRESSED;
        }
        private void EvaluateGUIClick()
        {
            if(markingActive && !markingDraging)
            {
                if (cursorX >= 5 && cursorX <= 102 && cursorY >= 10 && cursorY <= 57)
                {
                    if (leftMousebuttonClicked)
                    {
                        markingStartX = cursorX - 5;
                        markingStartY = cursorY - 10;

                        markingSpriteX = cursorX;
                        markingSpriteY = cursorY;

                        markingEndX = cursorX - 5;
                        markingEndY = cursorY - 10;

                        marking_visible = true;
                    }

                    if (leftMousebuttonHeld && !markingDraging)
                    {
                        markingEndX = cursorX - 5;
                        markingEndY = cursorY - 10;
                    }
                    else if (leftMouseButtonReleased)
                    {
                        markingDraging = true;
                        markingSpriteX = markingStartX + 5;
                        markingSpriteY = markingStartY + 10;
                    }
                }
            }
            else if(markingDraging)
            {
                if (leftMousebuttonClicked || leftMousebuttonHeld)
                {
                    markingSpriteX = cursorX - markingSprite.Width / 2;
                    markingSpriteY = cursorY - markingSprite.Height / 2;
                }
            }
            else if (leftMousebuttonClicked || leftMousebuttonHeld)
            {
                //color or brush picking
                if (cursorY == 2 || cursorY == 3)
                {
                    //foreground color
                    if (cursorX >= 1 && cursorX <= 32)
                    {
                        switch (cursorX)
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
                    else if (cursorX >= 40 && cursorX <= 71)
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
                    else if (cursorX >= 80 && cursorX <= 87)
                    {
                        switch (cursorX)
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
                else if (cursorX >= 5 && cursorX <= 102 && cursorY >= 10 && cursorY <= 57)
                {
                    if (cursorX - 5 < sprite.Width && cursorY - 10 < sprite.Height)
                    {
                        if (!colorPickerActive && !markingActive)
                        {
                            short color = (short)(backgroundColor << 4);
                            color += foregroundColor;
                            sprite.SetPixel(cursorX - 5 + spriteCursorX, cursorY - 10 + spriteCursorY, brush, color);
                        }
                        else if(markingActive)
                        {
                            
                        }
                        else if(colorPickerActive)
                        {
                            short colorToPick = sprite.GetColor(cursorX - 5 + spriteCursorX, cursorY - 10);
                            foregroundColor = (short)(colorToPick & 0x0F);
                            backgroundColor = (short)(colorToPick >> 4);
                            brush = sprite.GetChar(cursorX - 5 + spriteCursorX, cursorY - 10);
                        }
                    }
                }
                //colorPickerActive = false;
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
            string exportPath = tb_SaveName.content != "" ? @"Savefiles\" + tb_SaveName.content + ".txt" : @"Savefiles\" + "NewFile" + ".txt";

            using (StreamWriter outputfile = new StreamWriter(exportPath))
            {
                outputfile.Write($"{sprite.Width};{sprite.Height};");

                for (int j = 0; j < sprite.Height; j++)
                {
                    for (int i = 0; i < sprite.Width; i++)
                    {
                        outputfile.Write($"{sprite.GetChar(i, j)},");
                    }
                }
                outputfile.Write(";");

                for (int j = 0; j < sprite.Height; j++)
                {
                    for (int i = 0; i < sprite.Width; i++)
                    {
                        outputfile.Write($"{sprite.GetColor(i, j)},");
                    }
                }
            }

            saveFiles.Add(Path.GetFileName(exportPath));
            return true;
        }
        private bool BtnLoadClicked()
        {
            sprite = new Sprite("Savefiles\\" + saveFiles[lb_SavedFiles.selectedEntry]);

            return true;
        }
        private bool BtnColorPickerClicked()
        {
            markingActive = false;
            colorPickerActive = !colorPickerActive;
            return true;
        }
        private bool BtnMarkClicked()
        {
            colorPickerActive = false;
            markingActive = !markingActive;
            return true;
        }
        private bool BtnCopyClicked()
        {
            return true;
        }
        private bool BtnAbortClicked()
        {
            marking_visible = false;
            markingSprite = null;
            markingDraging = false;
            return true;
        }
        private bool BtnConfirmClicked()
        {
            if(markingSprite != null)
                sprite.AddSpriteToSprite(markingSpriteX - 5, markingSpriteY - 10, markingSprite.ReturnPartialSpriteInverted(0, 0, markingSprite.Width, markingSprite.Height));
            marking_visible = false;
            markingSprite = null;
            markingDraging = false;
            markingActive = false;
            
            return true;
        }
        #endregion

        #region DRAWING UI
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
        #endregion

        private class AnimationPreview
        {
            public int x, y;
            private int spriteW, spriteH;

            short foregroundColor, backgroundColor;

            Button btn_Start, btn_Stop, btn_Forward, btn_Backwards;
            TextBox tb_SpriteW, tb_SpriteH, tb_FrameDelay, tb_FrameCount;

            public Sprite outputSprite;
            int frameCounter = 0;
            bool loop = false;

            TimeSpan frameDelay = new TimeSpan(0, 0, 0, 0, 0);
            DateTime lastUpdate = DateTime.Now;

            public AnimationPreview(int x, int y, int spriteW = 16, int spriteH = 16, short backgroundColor = (short)COLOR.FG_BLACK, short foregroundColor = (short)COLOR.FG_WHITE)
            {
                this.x = x;
                this.y = y;
                this.spriteW = spriteW;
                this.spriteH = spriteH;
                this.backgroundColor = backgroundColor;
                this.foregroundColor = foregroundColor;

                btn_Backwards = new Button(x + 1, y + spriteH + 2, "<", method: BtnBackwardsClick);
                btn_Start = new Button(x + 5, y + spriteH + 2, "»", method: BtnStartClick);
                btn_Stop = new Button(x + 9, y + spriteH + 2, "■", method: BtnStopClick);
                btn_Forward = new Button(x + 13, y + spriteH + 2, ">", method: BtnForwardClick);

                tb_SpriteW = new TextBox(x + spriteW + 3, y, 3, "Width");
                tb_SpriteH = new TextBox(x + spriteW + 3, y + 5, 3, "Height");
                tb_FrameDelay = new TextBox(x + spriteW + 3, y + 10, 3, "Delay", content:"100");
                tb_FrameCount = new TextBox(x + spriteW + 3, y + 15, 5 , "#Frms", content:"1");
            }

            public void UpdateMouseInput(MOUSE_EVENT_RECORD r)
            {
                btn_Backwards.Update(r);
                btn_Start.Update(r);
                btn_Stop.Update(r);
                btn_Forward.Update(r);

                tb_SpriteW.UpdateSelection(r);
                tb_SpriteH.UpdateSelection(r);
                tb_FrameDelay.UpdateSelection(r);
                tb_FrameCount.UpdateSelection(r);
            }
            public void UpdateKeyInput(KeyState[] KeyStates, TimeSpan elapsedTime, Sprite sprite)
            {
                if(tb_FrameDelay.content != "")
                    frameDelay = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(tb_FrameDelay.content));

                if (frameDelay < DateTime.Now - lastUpdate && loop)
                {
                    lastUpdate = DateTime.Now;
                    frameCounter++;

                    if(tb_FrameCount.content != "")
                        if (frameCounter > Convert.ToInt32(tb_FrameCount.content))
                            frameCounter = 0;
                }
                tb_SpriteW.UpdateInput(KeyStates, elapsedTime);
                tb_SpriteH.UpdateInput(KeyStates, elapsedTime);
                tb_FrameDelay.UpdateInput(KeyStates, elapsedTime);
                tb_FrameCount.UpdateInput(KeyStates, elapsedTime);

                BuildSprite(sprite);
            }

            private void BuildSprite(Sprite sprite)
            {
                short color = (short)((foregroundColor << 4) + backgroundColor);
                outputSprite = new Sprite(8 + spriteW, 8 + spriteH);

                #region frame around spriteFrame
                //corners
                outputSprite.SetPixel(0, 0, '┌', color);
                outputSprite.SetPixel(spriteW + 1, 0, '┐', color);
                outputSprite.SetPixel(0, spriteH + 1, '└', color);
                outputSprite.SetPixel(spriteW + 1, spriteH + 1, '┘', color);

                for(int i = 0; i < spriteW; i++)
                {
                    //horizontal lines
                    outputSprite.SetPixel(1 + i, 0, '─', color);
                    outputSprite.SetPixel(1 + i, spriteH + 1, '─', color);
                    for (int j = 0; j < spriteH; j++)
                    {
                        //vertical lines
                        outputSprite.SetPixel(0, 1 + j, '│', color);
                        outputSprite.SetPixel(spriteW + 1, 1 + j, '│', color);
                    }
                }
                #endregion

                #region UI-Elements
                outputSprite.AddSpriteToSprite(1, spriteH + 2, btn_Backwards.outputSprite);
                outputSprite.AddSpriteToSprite(5, spriteH + 2, btn_Start.outputSprite);
                outputSprite.AddSpriteToSprite(9, spriteH + 2, btn_Stop.outputSprite);
                outputSprite.AddSpriteToSprite(13, spriteH + 2, btn_Forward.outputSprite);

                outputSprite.AddSpriteToSprite(spriteW + 3, 0, tb_SpriteW.outputSprite);
                outputSprite.AddSpriteToSprite(spriteW + 3, 5, tb_SpriteH.outputSprite);
                outputSprite.AddSpriteToSprite(spriteW + 3, 10, tb_FrameDelay.outputSprite);
                outputSprite.AddSpriteToSprite(spriteW + 3, 15, tb_FrameCount.outputSprite);
                #endregion

                #region animationFrame
                outputSprite.AddSpriteToSprite(1, 1, sprite.ReturnPartialSprite(frameCounter * spriteW, 0, spriteW, spriteH));

                #endregion
            }
            #region BtnClicks
            private bool BtnBackwardsClick()
            {
                frameCounter--;

                if (frameCounter < 0)
                    frameCounter = Convert.ToInt32(tb_FrameCount.content) - 1;
                return true;
            }
            private bool BtnStartClick()
            {
                loop = true;
                return true;
            }
            private bool BtnForwardClick()
            {
                frameCounter++;

                if (frameCounter > Convert.ToInt32(tb_FrameCount.content))
                    frameCounter = 0;

                return true;
            }
            private bool BtnStopClick()
            {
                loop = false;
                return true;
            }
            #endregion
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
