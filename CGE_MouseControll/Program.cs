using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEngine;

namespace CGE_Fonts
{
    class Fonts : GameConsole
    {
        public Fonts()
          : base(200, 120, "Fonts", fontwidth: 4, fontheight: 4)
        { }
        public override bool OnUserCreate()
        {
            TextWriter.LoadFont("fontsheet.txt", 7 , 9);

            return true;
        }

        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            Clear();

            DrawSprite(0, 0, TextWriter.GenerateTextSprite("Size: 1", TextWriter.Textalignment.Left, 1, backgroundColor: (short)COLOR.FG_YELLOW, foregroundColor: (short)COLOR.FG_RED));
            DrawSprite(0, 10, TextWriter.GenerateTextSprite("Size: 2", TextWriter.Textalignment.Left, 2));
            DrawSprite(0, 29, TextWriter.GenerateTextSprite("Size: 3", TextWriter.Textalignment.Left, 3));
            DrawSprite(0, 57, TextWriter.GenerateTextSprite("Size: 4", TextWriter.Textalignment.Left, 4));
            DrawSprite(20, 65, TextWriter.GenerateTextSprite("Alphatext", TextWriter.Textalignment.Left, 2, backgroundColor: (short)COLOR.TRANSPARENT, foregroundColor: (short)COLOR.FG_BLUE));

            DrawSprite(10, 90, TextWriter.GenerateTextSprite("Centered\ntext", TextWriter.Textalignment.Center, 1, backgroundColor: (short)COLOR.TRANSPARENT, foregroundColor: (short)COLOR.FG_BLUE));
            DrawSprite(90, 90, TextWriter.GenerateTextSprite("Text on the\nright", TextWriter.Textalignment.Right, 1, backgroundColor: (short)COLOR.TRANSPARENT, foregroundColor: (short)COLOR.FG_BLUE));

            return true;
        }
    }


    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

            using (var f = new Fonts())
                f.Start();
        }
    }
}
