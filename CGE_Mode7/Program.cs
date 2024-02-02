using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConsoleGameEngine;

namespace CGE_Mode7
{

    class ConsoleMode7 : GameConsole
    {
        Sprite sprite;
        private double fPlayerX = 13.7;          // Player Start Position
        private double fPlayerY = 5.09;
        private double fPlayerA = -26.7;

        double fNear = 0.005f;
        double fFar = 0.03f;
        double fFoVHalf = 3.14159f / 4.0f;


        public ConsoleMode7()
          : base(200, 120, "Mode7", fontwidth: 4, fontheight: 4)
        { }
        public override bool OnUserCreate()
        {
            sprite = new Sprite("mk_track.txt");
            return true;
        }

        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            // Control rendering parameters dynamically
            if (GetKeyState(ConsoleKey.Q).Held) fNear += 0.1 * (elapsedTime.TotalMilliseconds / 1000);
            if (GetKeyState(ConsoleKey.A).Held) fNear -= 0.1 * (elapsedTime.TotalMilliseconds / 1000);

            if (GetKeyState(ConsoleKey.W).Held) fFar += 0.1 * (elapsedTime.TotalMilliseconds / 1000);
            if (GetKeyState(ConsoleKey.S).Held) fFar -= 0.1 * (elapsedTime.TotalMilliseconds / 1000);

            if (GetKeyState(ConsoleKey.Y).Held) fFoVHalf += 0.1 * (elapsedTime.TotalMilliseconds / 1000);
            if (GetKeyState(ConsoleKey.X).Held) fFoVHalf -= 0.1 * (elapsedTime.TotalMilliseconds / 1000);



            if (GetKeyState(ConsoleKey.LeftArrow).Held)
                fPlayerA -= 1.0f * (elapsedTime.TotalMilliseconds / 1000);

            if (GetKeyState(ConsoleKey.RightArrow).Held)
                fPlayerA += 1.0f * (elapsedTime.TotalMilliseconds / 1000);

            if (GetKeyState(ConsoleKey.UpArrow).Held)
            {
                fPlayerX += Math.Sin(fPlayerA) * 0.01 * (elapsedTime.TotalMilliseconds); ;
                fPlayerY += Math.Cos(fPlayerA) * 0.01 * (elapsedTime.TotalMilliseconds); ;
            }
            if (GetKeyState(ConsoleKey.DownArrow).Held)
            {
                fPlayerX -= Math.Sin(fPlayerA) * 0.01 * (elapsedTime.TotalMilliseconds); ;
                fPlayerY -= Math.Cos(fPlayerA) * 0.01 * (elapsedTime.TotalMilliseconds); ;
            }

            Mode7(fPlayerY / 128, fPlayerX / 128, fPlayerA, fNear, fFar, fFoVHalf, sprite, false);

            return true;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

            using (var f = new ConsoleMode7())
                f.Start();

        }
    }
}
