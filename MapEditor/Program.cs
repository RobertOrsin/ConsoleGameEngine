using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEngine;
using static ConsoleGameEngine.GameConsole;
using static ConsoleGameEngine.NativeMethods;

namespace MapEditor
{
    class MapEditor : GameConsole
    {
        IntPtr inHandle;
        delegate void MyDelegate();
        MOUSE_EVENT_RECORD oldMouseState;

        ComboBox comboBox;
        TileBox tileBox = null;
        Sprite spriteSheet;
        List<string> saveFiles = new List<string>();


        public MapEditor()
          : base(200, 120, "Fonts", fontwidth: 6, fontheight: 6)
        { }
        public override bool OnUserCreate()
        {
            spriteSheet = new Sprite(1, 1);

            foreach (string file in Directory.EnumerateFiles(@"Savefiles\", "*.txt"))
                saveFiles.Add(Path.GetFileName(file));
            comboBox = new ComboBox(5, 1, 30, 50, "spriteSheet", saveFiles);

            spriteSheet = new Sprite("Savefiles\\" + saveFiles[0]);
            tileBox = new TileBox(5, 5, 4, 5, 16, 16, spriteSheet);




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
            DrawSprite(comboBox.x, comboBox.y, comboBox.outputSprite);
            if(tileBox != null)
                DrawSprite(tileBox.x, tileBox.y, tileBox.outputSprite);

            return true;
        }

        private void ConsoleListener_MouseEvent(MOUSE_EVENT_RECORD r)
        {
            comboBox.UpdateMouseInput(r);
            if(tileBox!= null)
                tileBox.Update(r);
        }


        class TileBox
        {
            public int x, y, spriteW, spriteH;
            int tilesPerRow, shownRows, firstRow = 0;
            int tileW, tileH;
            Sprite tileSheet;
            List<Sprite> tiles = new List<Sprite>();

            public Sprite outputSprite;
            Button btn_MoveUP, btn_MoveDOWN;

            short foregroundColor, backgroundColor;

            public TileBox(int x, int y, int tilesPerRow, int shownRows, int tileW, int tileH, Sprite tileSheet)
            {
                this.x = x;
                this.y = y;
                this.tilesPerRow = tilesPerRow;
                this.tileW = tileW;
                this.tileH = tileH;
                this.tileSheet = tileSheet;
                tiles = tileSheet.ReturnTileList(tileW, tileH, tileSheet.Width / tileW, tileSheet.Height / tileH);
                this.shownRows = shownRows;

                foregroundColor = 0x000F;
                backgroundColor = 0x0000;

                spriteW = 2 + tilesPerRow * tileW + tilesPerRow + 1 + 3; // 2 -> border, tileW times tileCount + tileCount + 1 -> selectionBorder + 3 ->scrollbar;
                spriteH = 2 + shownRows * tileH + tilesPerRow + 1;

                btn_MoveDOWN = new Button(x + spriteW - 3, y + spriteH - 4, "v", this.backgroundColor, this.foregroundColor, method:BtnMoveDownClicked);
                btn_MoveUP = new Button(x + spriteW - 3, y + 1, "^", this.backgroundColor, this.foregroundColor, method:BtnMoveUpClicked);

                outputSprite = BuildSprite();
            }

            private Sprite BuildSprite()
            {
               
                Sprite retSprite = new Sprite(spriteW, spriteH);

                short color = (short)((foregroundColor << 4) + backgroundColor);
                //frame
                for (int i = 1; i < retSprite.Width - 1; i++)
                {
                    retSprite.SetPixel(i, 0, (char)PIXELS.LINE_STRAIGHT_HORIZONTAL, color); //top
                    retSprite.SetPixel(i, retSprite.Height - 1, (char)PIXELS.LINE_STRAIGHT_HORIZONTAL, color); //bottom
                    for (int j = 1; j < retSprite.Height - 1; j++)
                    {
                        retSprite.SetPixel(0, j, (char)PIXELS.LINE_STRAIGHT_VERTICAL, color); //left
                        if (tileSheet.Height / (shownRows * tileH) > retSprite.Height - 2)
                            retSprite.SetPixel(retSprite.Width - 3, j, (char)PIXELS.LINE_STRAIGHT_VERTICAL, color); //border between entries and scrollbar
                        retSprite.SetPixel(retSprite.Width - 1, j, (char)PIXELS.LINE_STRAIGHT_VERTICAL, color); //right
                    }
                }
                retSprite.SetPixel(0, 0, (char)PIXELS.LINE_CORNER_TOP_LEFT, color);
                retSprite.SetPixel(0, retSprite.Height - 1, (char)PIXELS.LINE_CORNER_BOTTOM_LEFT, color);
                retSprite.SetPixel(retSprite.Width, 0, (char)PIXELS.LINE_CORNER_TOP_RIGHT, color);
                retSprite.SetPixel(retSprite.Width - 1, retSprite.Height, (char)PIXELS.LINE_CORNER_BOTTOM_RIGHT, color);

                //scrollbar
                if (tileSheet.Height / tileH > (shownRows) )
                {
                    retSprite.AddSpriteToSprite(spriteW - 3, 1, btn_MoveUP.outputSprite);
                    retSprite.AddSpriteToSprite(spriteW - 3, spriteH - 4, btn_MoveDOWN.outputSprite);

                    int scrollbarHeight = spriteH - 8;

                    int scrollbarPosition = (int)((double)scrollbarHeight * (((double)firstRow / (((double)tileSheet.Height / tileH) - 2.0))));

                    retSprite.SetPixel(spriteW - 2, scrollbarPosition + 4, '█', (short)COLOR.FG_DARK_GREY);
                }

                int entryCount = 0;
                //entrys
                for(int i = firstRow; i < shownRows + firstRow; i++) //rows
                {
                    for(int j = 0; j < tilesPerRow; j++) //columns
                    {
                        retSprite.AddSpriteToSprite(j * tileW + 1, (i - firstRow) * tileH + 1, tiles[i * tilesPerRow + j]);
                    }
                }




                return retSprite;
            }

            public void Update(MOUSE_EVENT_RECORD r)
            {
                btn_MoveDOWN.Update(r);
                btn_MoveUP.Update(r);
                outputSprite = BuildSprite();
            }

            private bool BtnMoveUpClicked()
            {
                firstRow -= 1;
                if(firstRow < 0)
                    firstRow = 0;

                outputSprite = BuildSprite();
                return true;
            }
            private bool BtnMoveDownClicked()
            {
                firstRow += 1;
                if (firstRow >= tileSheet.Height / tileH)
                    firstRow = tileSheet.Height / tileH - 1;

                outputSprite = BuildSprite();

                return true;
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

            using (var f = new MapEditor())
                f.Start();
        }
    }
}
