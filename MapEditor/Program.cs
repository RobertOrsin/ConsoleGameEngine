using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEngine;
using static ConsoleGameEngine.NativeMethods;

namespace MapEditor
{
    class MapEditor : GameConsole
    {
        public MapEditor()
          : base(200, 120, "Fonts", fontwidth: 4, fontheight: 4)
        { }
        public override bool OnUserCreate()
        {
            //Load sprites, setup variables and whatever
            return true;
        }
        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            //game loop, draw and evaluate inputs
            return true;
        }


        class TileBox
        {
            int x, y;
            int tilesPerRow, shownRows;
            int tileW, tileH;
            Sprite tileSheet;
            bool simple = true;

            public Sprite outputSprite;
            Button btn_MoveUP, btn_MoveDOWN;

            public TileBox(int x, int y, int tilesPerRow, int shownRows, int tileW, int tileH, Sprite tileSheet)
            {
                this.x = x;
                this.y = y;
                this.tilesPerRow = tilesPerRow;
                this.tileW = tileW;
                this.tileH = tileH;
                this.tileSheet = tileSheet;
                this.shownRows = shownRows;

                outputSprite = BuildSprite();
            }

            private Sprite BuildSprite()
            {
                int spriteW = 2 + tilesPerRow * tileW + tilesPerRow + 1 + 3; // 2 -> border, tileW times tileCount + tileCount + 1 -> selectionBorder + 3 ->scrollbar;
                int spriteH =  2 + shownRows * tileH + tilesPerRow + 1;
                Sprite retSprite = new Sprite(spriteW, spriteH);





                return retSprite;
            }

            public void Update(MOUSE_EVENT_RECORD r)
            {

                outputSprite = BuildSprite();
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
