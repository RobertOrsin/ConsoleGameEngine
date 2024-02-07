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
        int x, y, w, h;
        public List<string> entries = new List<string>();
        bool simple = false;
        short foregroundColor, backgroundColor;
        bool showOnlyFileName;
        public Sprite outputSprite = new Sprite(1, 1);

        public string selectedString = "";

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

            outputSprite = BuildSprite();
        }

        public void Update(MOUSE_EVENT_RECORD r)
        {
            
        }

        private Sprite BuildSprite()
        {
            Sprite retSprite = new Sprite(w, h);

            if (simple)
            {
                //frame

                //scrollbar

                //entrys
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
