using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEngine;


namespace CGE_3DEngine
{
    class ConsoleAnimation : GameConsole
    {
        _3DEngine _3DEngine;

        public ConsoleAnimation()
          : base(200, 120, "Animation", fontwidth: 4, fontheight: 4)
        { }
        public override bool OnUserCreate()
        {
            _3DEngine = new _3DEngine(120,200);


            return true;
        }

        public override bool OnUserUpdate(TimeSpan fElapsedTime)
        {
            Clear();


            _3DEngine.MovePlayer(fElapsedTime);
            _3DEngine.Draw3D();

            DrawSprite(0, 0, _3DEngine.screen);


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
