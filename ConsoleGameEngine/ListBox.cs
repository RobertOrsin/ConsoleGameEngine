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
        int w, h;
        public List<string> entries = new List<string>();
        bool simple = false;
        short foregroundColor, backgroundColor;
        bool showOnlyFileName;
        public Sprite outputSprite = new Sprite(1, 1);
        int firstEntry = 0;

        public int selectedEntry = 0;

        Button btn_MoveUP, btn_MoveDOWN;

        public ListBox(int x, int y, int w, int h, List<string> entries, bool simple = false, short backgroundColor = (short)COLOR.FG_BLACK, short foregroundColor = (short)COLOR.FG_WHITE, bool showOnlyFileName = false)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.entries = entries;
            this.simple = simple;
            this.backgroundColor = backgroundColor;
            this.foregroundColor = foregroundColor;
            this.showOnlyFileName = showOnlyFileName;

            btn_MoveDOWN = new Button(w - 3, 0, "v", this.backgroundColor, this.foregroundColor);
            btn_MoveUP = new Button(w - 3, h - 2, "^", this.backgroundColor, this.foregroundColor);

            outputSprite = BuildSprite();
        }

        public void Update(MOUSE_EVENT_RECORD r)
        {
            int mouseX = r.dwMousePosition.X, mouseY = r.dwMousePosition.Y;
            uint mouseState = r.dwButtonState;

            if (mouseState == MOUSE_EVENT_RECORD.FROM_LEFT_1ST_BUTTON_PRESSED)
            {
                if(mouseX >= x && mouseY >= y && mouseX <= x + w && mouseY <= y + h - 2)
                {
                    selectedEntry = mouseY - y - 1;
                    outputSprite = BuildSprite();
                }
            }

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
                    retSprite.SetPixel(i, 0, '-', color); //top
                    retSprite.SetPixel(i, retSprite.Height - 1, '-', color); //bottom
                    for (int j = 1; j < retSprite.Height - 1; j++)
                    {
                        retSprite.SetPixel(0, j, '|', color); //left
                        if (entries.Count > retSprite.Height - 2)
                            retSprite.SetPixel(retSprite.Width - 3, j, '|', color); //border between entries and scrollbar
                        retSprite.SetPixel(retSprite.Width - 1, j, '|', color); //right
                    }
                }
                retSprite.SetPixel(0, 0, '+', color);
                retSprite.SetPixel(0, retSprite.Height - 1, '+', color);
                retSprite.SetPixel(retSprite.Width , 0, '+', color);
                retSprite.SetPixel(retSprite.Width - 1, retSprite.Height, '+', color);

                //scrollbar
                if (entries.Count > retSprite.Height - 2)
                {
                    retSprite.AddSpriteToSprite(btn_MoveUP.x, btn_MoveUP.y, btn_MoveUP.outputSprite);
                    retSprite.AddSpriteToSprite(btn_MoveDOWN.x, btn_MoveDOWN.y, btn_MoveDOWN.outputSprite);
                }

                int entryCount = 0;
                //entrys
                for(int i = firstEntry; i < entries.Count && i < h - 2; i++)
                {
                    color = i == selectedEntry ? (short)((foregroundColor << 4) + backgroundColor) : (short)((backgroundColor << 4) + foregroundColor);

                    for (int j = 0; j < entries[i].Length && j < w - 4; j++)
                    {
                   
                        retSprite.SetPixel(j + 1, entryCount + 1, entries[i][j], color);
                    }

                    entryCount++;
                }
                foreach(string entry in entries)
                {
                    
                }
            }
            else
            {
                //frame

                //scrollbar

                //entrys
            }

            return retSprite;
        }
    }
}
