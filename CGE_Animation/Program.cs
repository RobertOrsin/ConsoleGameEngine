using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEngine;

namespace CGE_Animation
{
    class ConsoleAnimation : GameConsole
    {
        animation diddysAnimation;


        public ConsoleAnimation()
          : base(200, 120, "Animation", fontwidth: 4, fontheight: 4)
        { }
        public override bool OnUserCreate()
        {
            diddysAnimation = new animation("diddy_idle.txt", new TimeSpan(0, 0, 0, 0, 100), 37, 48);

            return true;
        }

        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            diddysAnimation.Update();

            DrawSprite(0, 0, diddysAnimation.outputSprite);

            return true;
        }
    }

        internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

            using (var f = new ConsoleAnimation())
                f.Start();
        }
    }
}
