using System;
using System.IO;
using BigGustave;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using ConsoleGameEngine;

namespace PNGToSpriteEditor
{

    class PNGToSpriteEditor : GameConsole
    {
        string importPath;
        string exportPath;

        Rectangle drawingArea;
        int selectedX = 0, selectedY = 0;
        short selectedFG = 0x00;
        short selectedBG = 0x00;
        int selectedBrush = 0;
        TimeSpan buttonDelay = new TimeSpan();
        TimeSpan buttonTime;

        int maxDisplayedWidth = 158;
        int maxDisplayedHeight = 76;

        int partialSpriteX = 0;
        int partialSpriteY = 0;

        public PNGToSpriteEditor(string file)
            : base(160, 80, "Editor", fontwidth: 12, fontheight: 12) //80,60,12
        {
            importPath = file;
            exportPath = Path.ChangeExtension(file, ".txt");
        }

        public Sprite sprite;

        public override bool OnUserCreate()
        {
            //Check if file is txt or png
            switch(Path.GetExtension(importPath))
            {
                case ".png":
                    Png png = Png.Open(importPath);

                    sprite = new Sprite(png.Width, png.Height);

                    for (int x = 0; x < png.Width; x++)
                    {
                        for (int y = 0; y < png.Height; y++)
                        {
                            byte red = png.GetPixel(x, y).R;
                            byte green = png.GetPixel(x, y).G;
                            byte blue = png.GetPixel(x, y).B;

                            short col = ClosedConsoleColor3Bit(red, green, blue, out char pixel);

                            //use white as alpha value
                            //if (col == 0x00F0 || col == 0x0007)
                            //    sprite.SetPixel(x, y, ' ', 0x0000);
                            //else
                                sprite.SetPixel(x, y, pixel, col);
                        }
                    }
                    break;
                case ".txt": 
                    sprite = new Sprite(importPath);                  
                    break;
                default:
                    string[] splits = importPath.Split(':');
                    if(splits.Length != 2 )
                        return false;

                    sprite = new Sprite(Convert.ToInt32(importPath.Split(':')[1].Split(';')[0]), Convert.ToInt32(importPath.Split(':')[1].Split(';')[0]));
                    exportPath = AppDomain.CurrentDomain.BaseDirectory + $"{DateTime.Now.ToString().Replace('.','_').Replace(':','_')}.txt"; 
                    break;
            }
            buttonTime = new TimeSpan(0, 0, 0, 0, 100);

            return true;
        }
        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            buttonDelay += elapsedTime;
            #region inputs
            //Save sprite
            if (GetKeyState(ConsoleKey.P).Pressed)
            {
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
            }
            //pixel selector movement
            if (GetKeyState(ConsoleKey.W).Held && buttonDelay >= buttonTime)
            {
                selectedY -= 1;
                if (selectedY < 0)
                    selectedY = 0;

                buttonDelay = new TimeSpan();
            }
            if (GetKeyState(ConsoleKey.A).Held && buttonDelay >= buttonTime)
            {
                selectedX -= 1;
                if (selectedX < 0)
                    selectedX = 0;

                buttonDelay = new TimeSpan();
            }
            if (GetKeyState(ConsoleKey.S).Held && buttonDelay >= buttonTime)
            {
                selectedY += 1;
                if (selectedY >= sprite.Height)
                    selectedY = sprite.Height - 1;

                buttonDelay = new TimeSpan();
            }
            if (GetKeyState(ConsoleKey.D).Held && buttonDelay >= buttonTime)
            {
                selectedX += 1;
                if (selectedX >= sprite.Width)
                    selectedX = sprite.Width - 1;

                buttonDelay = new TimeSpan();
            }
            //color and brush selector
            if (GetKeyState(ConsoleKey.NumPad7).Held && buttonDelay >= buttonTime)
            {
                selectedFG -= 0x1;
                buttonDelay = new TimeSpan();

                if (selectedFG < 0x0000)
                    selectedFG = 0x000F;
            }
            if (GetKeyState(ConsoleKey.NumPad8).Held && buttonDelay >= buttonTime)
            {
                selectedFG += 0x1;
                buttonDelay = new TimeSpan();

                if (selectedFG > 0x000F)
                    selectedFG = 0x0000;
            }
            if (GetKeyState(ConsoleKey.NumPad4).Held && buttonDelay >= buttonTime)
            {
                selectedBG -= 0x1;
                buttonDelay = new TimeSpan();

                if (selectedBG < 0x0000)
                    selectedBG = 0x000F;
            }
            if (GetKeyState(ConsoleKey.NumPad5).Held && buttonDelay >= buttonTime)
            {
                selectedBG += 0x1;
                buttonDelay = new TimeSpan();

                if (selectedBG > 0x000F)
                    selectedBG = 0x0000;
            }
            if (GetKeyState(ConsoleKey.NumPad1).Held && buttonDelay >= buttonTime)
            {
                selectedBrush -= 1;
                buttonDelay = new TimeSpan();

                if (selectedBrush < 0)
                    selectedBrush = 4;
            }
            if (GetKeyState(ConsoleKey.NumPad2).Held && buttonDelay >= buttonTime)
            {
                selectedBrush += 1;
                buttonDelay = new TimeSpan();

                if (selectedBrush > 4)
                    selectedBrush = 0;
            }
            //fill screen with selected color and brush
            if (GetKeyState(ConsoleKey.F).Pressed)
            {
                short Targetcolor = (short)(selectedBG << 4);
                Targetcolor += selectedFG;

                PIXELS pixel = PIXELS.PIXEL_NONE;

                switch (selectedBrush)
                {
                    case 0:
                        pixel = (PIXELS.PIXEL_NONE); break;
                    case 1:
                        pixel = (PIXELS.PIXEL_QUARTER); break;
                    case 2:
                        pixel = (PIXELS.PIXEL_HALF); break;
                    case 3:
                        pixel = (PIXELS.PIXEL_THREEQUARTERS); break;
                    case 4:
                        pixel = (PIXELS.PIXEL_SOLID); break;
                }

                for (int x = 0; x < sprite.Width; x++)
                {
                    for (int y = 0; y < sprite.Height; y++)
                    {
                        sprite.SetPixel(x, y, (char)pixel, Targetcolor);
                    }
                }
            }
            //clear selected pixel
            if (GetKeyState(ConsoleKey.R).Pressed)
            {
                sprite.SetColor(selectedX, selectedY, (short)COLOR.BG_BLACK);
                sprite.SetChar(selectedX, selectedY, ' ');
            }
            //set pixel
            if (GetKeyState(ConsoleKey.Spacebar).Held)
            {
                short Targetcolor = (short)(selectedBG << 4);
                Targetcolor += selectedFG;

                switch (selectedBrush)
                {
                    case 0:
                        sprite.SetPixel(selectedX, selectedY, (char)(PIXELS.PIXEL_NONE), Targetcolor); break;
                    case 1:
                        sprite.SetPixel(selectedX, selectedY, (char)(PIXELS.PIXEL_QUARTER), Targetcolor); break;
                    case 2:
                        sprite.SetPixel(selectedX, selectedY, (char)(PIXELS.PIXEL_HALF), Targetcolor); break;
                    case 3:
                        sprite.SetPixel(selectedX, selectedY, (char)(PIXELS.PIXEL_THREEQUARTERS), Targetcolor); break;
                    case 4:
                        sprite.SetPixel(selectedX, selectedY, (char)(PIXELS.PIXEL_SOLID), Targetcolor); break;
                }
            }
            //sprite navigation (sprite bigger then screen)
            if(GetKeyState(ConsoleKey.UpArrow).Held && buttonDelay >= buttonTime)
            {
                buttonDelay = new TimeSpan();
                partialSpriteY -= 5;

                if(partialSpriteY < 0)
                    partialSpriteY = 0;
            }
            if (GetKeyState(ConsoleKey.DownArrow).Held && buttonDelay >= buttonTime)
            {
                buttonDelay = new TimeSpan();
                partialSpriteY += 5;

                if (partialSpriteY > sprite.Height - maxDisplayedHeight - 1)
                    partialSpriteY = sprite.Height - maxDisplayedHeight;
            }
            if (GetKeyState(ConsoleKey.LeftArrow).Held && buttonDelay >= buttonTime)
            {
                buttonDelay = new TimeSpan();
                partialSpriteX -= 5;

                if (partialSpriteX < 0)
                    partialSpriteX = 0;
            }
            if (GetKeyState(ConsoleKey.RightArrow).Held && buttonDelay >= buttonTime)
            {
                buttonDelay = new TimeSpan();
                partialSpriteX += 5;

                if (partialSpriteX > sprite.Width - maxDisplayedWidth)
                    partialSpriteX = sprite.Width - maxDisplayedWidth;
            }
            #endregion

            #region GUI
            //frame
            for (int x = 0; x < Width; x++)
            {
                for(int y = 0; y < Height; y++)
                {
                    if(y == 0  || y == 2 || y == Height - 5 || y == Height - 1) //frame top, line after headline, line above menu, frame bottom
                    {
                        SetChar(x, y, '#');
                    }

                    if (x == 0 || x == Width - 1) //frame left and right
                        SetChar(x, y, '#');
                }
            }

            drawingArea = new Rectangle(1,4,Width - 2,Height - 8);

            //headline
            Print(1, 1, "                                               PNG to Sprite Editor                                                            ");
            //legend
            Print(2, Height - 4, "move cursor:  apply change:  fill:  delete:  Foreground:  Background:  Brush:  MoveSprite:");
            Print(2, Height - 3, "-->  W,A,S,D       Spacebar      F        R       7/8           4/5     1/2      ArrowKeys");
            SetChar(57, Height - 3, (char)PIXELS.PIXEL_SOLID, selectedFG);
            SetChar(68, Height - 3, (char)PIXELS.PIXEL_SOLID, (short)(selectedBG));

            short color = (short)(selectedBG << 4);
            color += selectedFG;

            switch (selectedBrush)
            {
                case 0:
                    SetChar(78, Height - 3, (char)(PIXELS.PIXEL_NONE), color); break;
                case 1:
                    SetChar(78, Height - 3, (char)(PIXELS.PIXEL_QUARTER), color); break;
                case 2:
                    SetChar(78, Height - 3, (char)(PIXELS.PIXEL_HALF), color); break;
                case 3:
                    SetChar(78, Height - 3, (char)(PIXELS.PIXEL_THREEQUARTERS), color); break;
                case 4:
                    SetChar(78, Height - 3, (char)(PIXELS.PIXEL_SOLID), color); break;

            }
            #endregion

            #region sprite
            int spriteXTop = drawingArea.Width / 2 - sprite.Width / 2;
            int spriteYTop = drawingArea.Height / 2 - sprite.Height / 2 + drawingArea.Y;
            if (sprite.Width <= maxDisplayedWidth && sprite.Height <= maxDisplayedHeight) //sprite fits screen
                DrawSprite(spriteXTop, spriteYTop, sprite);

            else //sprite is too big, show only a part
                DrawPartialSprite(2, 4, sprite, partialSpriteX, partialSpriteY, maxDisplayedWidth, maxDisplayedHeight);
            
            //draw koordinates on x an y axis
            for (int x = 0; x < sprite.Width; x++)
                Print(spriteXTop + x, spriteYTop - 1, $"{(x % 10)}");
            for (int y = 0; y < sprite.Height; y++)
                Print(spriteXTop - 1, spriteYTop + y, $"{(y % 10)}");

            //draw selector-lines
            for (int x = 0; x <= selectedX; x++)
            {
                SetChar(spriteXTop - 1 + x, spriteYTop + selectedY, '=');
            }
            for (int y = 0; y <= selectedY; y++)
            {
                SetChar(spriteXTop + selectedX, spriteYTop - 1 + y, '|');
            }
            #endregion

            return true;
        }

        static short ClosedConsoleColor3Bit(byte r, byte g, byte b, out char pixel)
        {
            short sixBitValue = (short)((r / 85) << 4 | (g / 85) << 2 | (b / 85));

            switch(sixBitValue)
            {
                case 0xFF: //white
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x000F;

                case 0x00:  //black
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0000;
                case 0x15: //dark grey
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0008;
                case 0x3F: //light grey
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0007;

                case 0x30: //red
                case 0x31:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x000C;
                case 0x20: //dark red
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0004;
                case 0x10: //darker red
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x000C;
                case 0x25: //blueish red
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x009C;

                case 0x03: //blue
                case 0x17:
                case 0x07:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0009;
                case 0x02: //dark blue
                case 0x06:
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x0001;
                case 0x01: //darker blue
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x0009;
                case 0x2F: //cornflower blue
                case 0x1B:
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x009F;
                case 0x05: //petrol
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x009A;
                case 0x0B: //light blue
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x00F9;

                case 0x0C: //green
                case 0x1C:
                case 0x1D:
                case 0x08:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x000A;
                case 0x18: //dark green
                case 0x19:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0002;
                case 0x04: //darker green
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x000A;
                case 0x14: //darkest green
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x000A;
                case 0x2C: //yellowish green
                case 0x2D:
                case 0x2E:
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x00EA;
                case 0x0A: //blueish green
                case 0x1A:
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x009A;

                case 0x0D: //leaf green
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x009A;
                case 0x0E: //frog green
                case 0x1E:
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x001A;
                case 0x0F: //cyan
                case 0x1F:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x000B;


                case 0x28: //dark yellow
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0006;
                case 0x3C: //yellow
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x00F6;
                case 0x3D: //light yellow
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x000E;
                case 0x3E: //lighter yellow
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x00FE;

                case 0x11: //dark purple
                case 0x26:
                    pixel = (char)PIXELS.PIXEL_HALF;
                    return 0x001C;
                case 0x12: //purple
                case 0x16:
                case 0x22:
                    pixel = (char)PIXELS.PIXEL_HALF;
                    return 0x009C;
                case 0x13: //light purple
                case 0x23:
                case 0x27:
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x009C;

                case 0x33: //magenta
                case 0x37:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x000D;
                case 0x32: //dark magenta
                case 0x36:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0005;

                case 0x38: //orange
                case 0x34:
                case 0x35:
                    pixel = (char)PIXELS.PIXEL_HALF;
                    return 0x00EC;

                case 0x24: //brown
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x0096;
                case 0x29: //skintone'ish?
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x00EC;

                case 0x2A: //piggy colored
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x00FC;

                case 0x2B: //pink
                    pixel = (char)PIXELS.PIXEL_HALF;
                    return 0x00FC;

                case 0x21: //dark pink
                    pixel = (char)PIXELS.PIXEL_HALF;
                    return 0x00F4;

                default:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x000F;

            }
        }
    }
    internal class Program
    {
        static Dictionary<byte, string> AnsiLookup;

        static void Main(string[] args)
        {
            Encoding cp437 = Encoding.GetEncoding(437);

            Console.WriteLine("Drag and drop a PNG- or Sprite (.txt)-File in here and press Enter");
            Console.WriteLine("For a new file type New:Width;Height");
            Console.Write("Your file here: ");

            string file = Console.ReadLine();

            using (var f = new PNGToSpriteEditor(file))
                f.Start();

        }
    }
}
