using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConsoleGameEngine.GameConsole;
using static ConsoleGameEngine.NativeMethods;

namespace ConsoleGameEngine
{
    public class ListBox
    {
        public int x, y;
        public int w, h;
        public List<string> entries = new List<string>();
        bool simple = false;
        short foregroundColor, backgroundColor;
        public Sprite outputSprite = new Sprite(1, 1);
        int firstEntry = 0;

        public int selectedEntry = 0;

        Button btn_MoveUP, btn_MoveDOWN;

        public ListBox(int x, int y, int w, int h, List<string> entries, bool simple = false, short backgroundColor = (short)COLOR.FG_BLACK, short foregroundColor = (short)COLOR.FG_WHITE)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.entries = entries;
            this.simple = simple;
            this.backgroundColor = backgroundColor;
            this.foregroundColor = foregroundColor;

            if (simple)
            {
                btn_MoveDOWN = new Button(x + w - 3, y + h - 4, "v", this.backgroundColor, this.foregroundColor);
                btn_MoveUP = new Button(x + w - 3, y + 1, "^", this.backgroundColor, this.foregroundColor);
            }
            else
            {
                btn_MoveDOWN = new Button(x + w - 3, y + h - 4, TextWriter.GenerateTextSprite("v",TextWriter.Textalignment.Center,1));
                btn_MoveUP = new Button(x + w - 3, y + 1, TextWriter.GenerateTextSprite("A", TextWriter.Textalignment.Center, 1));
            }

            btn_MoveDOWN.OnButtonClicked(Btn_MoveDOWNClicked);
            btn_MoveUP.OnButtonClicked(Btn_MoveUPClicked);

            outputSprite = BuildSprite();
        }

        public void Update(MOUSE_EVENT_RECORD r)
        {
            int mouseX = r.dwMousePosition.X, mouseY = r.dwMousePosition.Y;
            uint mouseState = r.dwButtonState;

            if (mouseState == MOUSE_EVENT_RECORD.FROM_LEFT_1ST_BUTTON_PRESSED)
            {
                if (simple)
                {
                    if (mouseX >= x && mouseY >= y && mouseX <= x + w - 4 && mouseY <= y + h - 2)
                    {
                        selectedEntry = mouseY - y - 1 + firstEntry;
                    }
                }
                else
                {
                    if (mouseX >= x && mouseY >= y && mouseX <= x + w - 4 && mouseY <= y + h - 2)
                    {
                        selectedEntry = (mouseY - y) / TextWriter.height + firstEntry;
                    }
                }
            }

            btn_MoveDOWN.Update(r);
            btn_MoveUP.Update(r);

            outputSprite = BuildSprite();
        }

        private Sprite BuildSprite()
        {
            Sprite retSprite = new Sprite(w, h);

            short color = (short)((foregroundColor << 4) + backgroundColor);

            if (simple)
            {
                //frame
                for (int i = 1; i < retSprite.Width - 1; i++)
                {
                    retSprite.SetPixel(i, 0, (char)PIXELS.LINE_STRAIGHT_HORIZONTAL, color); //top
                    retSprite.SetPixel(i, retSprite.Height - 1, (char)PIXELS.LINE_STRAIGHT_HORIZONTAL, color); //bottom
                    for (int j = 1; j < retSprite.Height - 1; j++)
                    {
                        retSprite.SetPixel(0, j, (char)PIXELS.LINE_STRAIGHT_VERTICAL, color); //left
                        if (entries.Count > retSprite.Height - 2)
                            retSprite.SetPixel(retSprite.Width - 3, j, (char)PIXELS.LINE_STRAIGHT_VERTICAL, color); //border between entries and scrollbar
                        retSprite.SetPixel(retSprite.Width - 1, j, (char)PIXELS.LINE_STRAIGHT_VERTICAL, color); //right
                    }
                }
                retSprite.SetPixel(0, 0, (char)PIXELS.LINE_CORNER_TOP_LEFT, color);
                retSprite.SetPixel(0, retSprite.Height - 1, (char)PIXELS.LINE_CORNER_BOTTOM_LEFT, color);
                retSprite.SetPixel(retSprite.Width , 0, (char)PIXELS.LINE_CORNER_TOP_RIGHT, color);
                retSprite.SetPixel(retSprite.Width - 1, retSprite.Height, (char)PIXELS.LINE_CORNER_BOTTOM_RIGHT, color);

                //scrollbar
                if (entries.Count > retSprite.Height - 2)
                {
                    retSprite.AddSpriteToSprite(w - 3, 1, btn_MoveUP.outputSprite);
                    retSprite.AddSpriteToSprite(w - 3, h - 4, btn_MoveDOWN.outputSprite);

                    int scrollbarHeight = h - 8;

                    int scrollbarPosition = (int)((double)scrollbarHeight * (((double)firstEntry / (((double)entries.Count) - ((double)h - 2.0)))));

                    retSprite.SetPixel(w - 2, scrollbarPosition + 4, '█', (short)COLOR.FG_DARK_GREY);
                }

                int entryCount = 0;
                //entrys
                for(int i = firstEntry; i < entries.Count && (i - firstEntry) < h - 2; i++)
                {
                    color = i == selectedEntry ? (short)((foregroundColor << 4) + backgroundColor) : (short)((backgroundColor << 4) + foregroundColor);

                    for (int j = 0; j < entries[i].Length && j < w - 4; j++)
                    {
                        retSprite.SetPixel(j + 1, entryCount + 1, entries[i][j], color);
                    }

                    entryCount++;
                }
            }
            else
            {
                //frame
                for (int i = 0; i < retSprite.Width - 1; i++)
                {
                    retSprite.SetPixel(i, 0, (char)PIXELS.PIXEL_SOLID, (short)COLOR.FG_WHITE); //top
                    retSprite.SetPixel(i, retSprite.Height - 1, (char)PIXELS.PIXEL_SOLID, (short)COLOR.FG_WHITE); //bottom
                    for (int j = 0; j < retSprite.Height - 1; j++)
                    {
                        retSprite.SetPixel(0, j, (char)PIXELS.PIXEL_SOLID, (short)COLOR.FG_WHITE); //left
                        if (entries.Count > retSprite.Height - 2)
                            retSprite.SetPixel(retSprite.Width - 3, j, (char)PIXELS.PIXEL_SOLID, (short)COLOR.FG_WHITE); //border between entries and scrollbar
                        retSprite.SetPixel(retSprite.Width - 1, j, (char)PIXELS.PIXEL_SOLID, (short)COLOR.FG_WHITE); //right
                    }
                }
                //scrollbar
                if (entries.Count * TextWriter.width > retSprite.Height - 2)
                {
                    retSprite.AddSpriteToSprite(w - btn_MoveUP.outputSprite.Width, 1, btn_MoveUP.outputSprite);
                    retSprite.AddSpriteToSprite(w - btn_MoveUP.outputSprite.Width, h - btn_MoveUP.outputSprite.Height, btn_MoveDOWN.outputSprite);

                    int scrollbarHeight = h - 8;

                    int scrollbarPosition = (int)((double)scrollbarHeight * (((double)firstEntry / (((double)entries.Count) - ((double)h - 2.0)))));

                    retSprite.SetPixel(w - 2, scrollbarPosition + 4, '█', (short)COLOR.FG_DARK_GREY);
                }

                //entrys
                int entryCount = 0;
                for (int i = firstEntry; i < entries.Count && (i - firstEntry) < h - 2; i++)
                {
                    bool selected = i == selectedEntry;
                    if(!selected) retSprite.AddSpriteToSprite(1, (entryCount * TextWriter.height) + 1, TextWriter.GenerateTextSprite(entries[i], TextWriter.Textalignment.Left, 1,(short)COLOR.BG_BLACK,(short)COLOR.FG_WHITE));
                    else retSprite.AddSpriteToSprite(1, (entryCount * TextWriter.height) + 1, TextWriter.GenerateTextSprite(entries[i], TextWriter.Textalignment.Left, 1, (short)COLOR.BG_WHITE, (short)COLOR.FG_BLACK));
                    for (int j = 0; j < entries[i].Length && j < w - 4; j++)
                    {
                        
                        //SetPixel(j + 1, entryCount + 1, entries[i][j], color);
                    }

                    entryCount++;
                }
            }

            return retSprite;
        }

        private bool Btn_MoveUPClicked()
        {
            firstEntry -= 1;
            if(firstEntry < 0)
                firstEntry = 0;

            outputSprite = BuildSprite();
            return true;
        }
        private bool Btn_MoveDOWNClicked()
        {
            firstEntry += 1;
            if( firstEntry > entries.Count - (h - 2) )
                firstEntry = entries.Count - (h - 2);

            outputSprite = BuildSprite();
            return true;
        }
    }
}
