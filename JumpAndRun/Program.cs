using System;
using System.Collections.Generic;
using ConsoleGameEngine;
using static ConsoleGameEngine.Other.NativeMethods;
using Windows.Foundation;
using ConsoleGameEngine.Other;


namespace JumpAndRun
{
    class JumpAndRun : GameConsole
    {
        Player player;
        Level level;
        TimeSpan keyInputDelay = new TimeSpan(), keyInputTime = new TimeSpan(0, 0, 0, 0, 120);
        IntPtr inHandle;
        int cursorX = 0, cursorY = 0;
        bool leftMousebuttonClicked = false, mouseWheelClicked = false, rightMousebuttonClicked = false;

        bool startLevel = false;
        int points = 0;
        int lastHeight = 0;

        public JumpAndRun()
          : base(200, 120, "JumpAndRun", fontwidth: 4, fontheight: 4)
        { }
        public override bool OnUserCreate()
        {
            inHandle = NativeMethods.GetStdHandle(NativeMethods.STD_INPUT_HANDLE);
            uint mode = 0;
            NativeMethods.GetConsoleMode(inHandle, ref mode);
            mode &= ~NativeMethods.ENABLE_QUICK_EDIT_MODE; //disable
            mode |= NativeMethods.ENABLE_WINDOW_INPUT; //enable (if you want)
            mode |= NativeMethods.ENABLE_MOUSE_INPUT; //enable
            NativeMethods.SetConsoleMode(inHandle, mode);

            ConsoleListener.MouseEvent += ConsoleListener_MouseEvent;

            ConsoleListener.Start();

            TextWriter.InitTextWriter();


            player = new Player();
            level = new Level();
            player.LoadAnimation("runnin ninja.txt");
            //Load sprites, setup variables and whatever
            return true;
        }
        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            keyInputDelay += elapsedTime;
            player.Update(KeyStates, elapsedTime, this);

            if (startLevel)
            {
                level.Update(elapsedTime);
            }
            
            Clear();
            DrawSprite((int)player.xPosition, (int)player.yPosition, player.outputSprite);
            DrawSprite(0, 0, TextWriter.GenerateTextSprite($"   NINJA TOWER   {level.points} ", TextWriter.Textalignment.Left, 1));

            //draw plattforms
            foreach (Level.Plattform p in level.plattforms)
            {
                DrawSprite(p.x, p.y, new Sprite(p.l, 1, GameConsole.COLOR.BG_DARK_GREEN));
            }

            if(player.yPosition < 50) startLevel = true;
            if (player.yPosition > 120) startLevel = false;

            return true;
        }

        private void ConsoleListener_MouseEvent(MOUSE_EVENT_RECORD r)
        {
            cursorX = r.dwMousePosition.X;
            cursorY = r.dwMousePosition.Y;

            leftMousebuttonClicked = r.dwButtonState == MOUSE_EVENT_RECORD.FROM_LEFT_1ST_BUTTON_PRESSED;
            mouseWheelClicked = r.dwButtonState == MOUSE_EVENT_RECORD.FROM_LEFT_2ND_BUTTON_PRESSED;
            rightMousebuttonClicked = r.dwButtonState == MOUSE_EVENT_RECORD.RIGHTMOST_BUTTON_PRESSED;
        }
    }

    class Level
    {
        public List<Plattform> plattforms;
        public List<Plattform> walls;
        TimeSpan _elapsedTime = new TimeSpan();
        TimeSpan updateDelay = new TimeSpan(0, 0, 0, 0, 40);
        const int MAXplattformcount = 7;
        Random random = new Random();
        Rect boundaries = new Rect(0,9,200,111);
        public int points = 0;

        public Level()
        {
            plattforms = new List<Plattform>();
            walls = new List<Plattform>();

            //ground-plattform
            plattforms.Add(new Plattform { x = 0, y = 120, l = 200 });
            //intro plattforms
            plattforms.Add(new Plattform { x = 50, y = 70, l = 70 });
            plattforms.Add(new Plattform { x = 90, y = 100, l = 70 });
            plattforms.Add(new Plattform { x = 150, y = 40, l = 35 });
            plattforms.Add(new Plattform { x = 130, y = 55, l = 20 });

            //walls
            walls.Add(new Plattform { x = 0, y = 0, l = 120 });
            walls.Add(new Plattform { x = 200, y = 0, l = 120 });
        }

        public void Update(TimeSpan elapsedTime)
        {
            _elapsedTime += elapsedTime;

            if( _elapsedTime > updateDelay )
            {
                _elapsedTime = new TimeSpan();
                points++;

                //move plattforms down
                List<Plattform> updatedPlattforms = new List<Plattform>();
                for(int i = 0; i < plattforms.Count; i++)
                {
                    Plattform p = plattforms[i];
                    p.y += 1;

                    if(p.y <= 120) updatedPlattforms.Add(p);
                }
                plattforms = updatedPlattforms;
                //check if new plattforms can be added
                for(int x = plattforms.Count; x < MAXplattformcount; x++)
                {
                    plattforms.Add(new Plattform { x = random.Next(0,200), y = random.Next((int)boundaries.Top, 50), l = random.Next(20,70) });
                }
            }
        }

        public struct Plattform
        {
            public int x;
            public int y;
            public int l;

            public (int left, int right, int y) Bounds() => (x, x + l, y);
        }
    }



    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            using (var f = new JumpAndRun())
                f.Start();
        }
    }
}

