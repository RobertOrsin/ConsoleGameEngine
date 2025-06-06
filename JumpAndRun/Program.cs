using System;
using System.Collections.Generic;
using System.Windows;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using ConsoleGameEngine;
using static ConsoleGameEngine._3DEngine;
using static ConsoleGameEngine.GameConsole;
using static ConsoleGameEngine.NativeMethods;


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
          : base(200, 120, "Fonts", fontwidth: 4, fontheight: 4)
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

            TextWriter.LoadFont("fontsheet.txt", 6, 8);


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

            //draw walls
            //foreach (Level.Plattform p in level.walls)
            //{
            //    DrawSprite(p.x, p.y, new Sprite(1, p.l, GameConsole.COLOR.BG_DARK_GREEN));
            //}


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

    class Player
    {
        public double xPosition, yPosition, xVelocity, yVelocity;
        public Sprite outputSprite;
        private Sprite spriteSheet;
        private animation walkingAnimation;

        private bool airjumpused = false;

        private double velocityMax = 30;

        private const double walkSpeed = 2, runSpeed = 5, fallSpeed = 10,  acceleration = 0.5, gravity_acceleration = 2.0;
        private double playerSpeedX = 0.0, playerSpeedY = 0.0;

        private int sign = 0;

        public Player()
        {
            outputSprite = new Sprite(8, 8);
            xPosition = 100.0;
            yPosition = 50.0;
        }

        public void LoadAnimation(string file)
        {
            walkingAnimation = new animation("running ninja.txt", new TimeSpan(0, 0, 0, 0, 100), 16, 16);
            spriteSheet = new Sprite("running ninja.txt");
        }

        public void Update(KeyState[] KeyStates, TimeSpan elapsedTime, GameConsole gameConsole)
        {
            walkingAnimation.Update();
            BuildSprite();

            #region reset
            if (GetKeyState(ConsoleKey.R).Pressed)
            {
                xPosition = 100.0;
                yPosition = 50.0;
                playerSpeedX = 0.0;
                playerSpeedY = 0.0;
            }

            #endregion

            #region horizontal movement
            if (GetKeyState(ConsoleKey.A).Held)
            {
                playerSpeedX -= acceleration;
                playerSpeedX = ClampF(playerSpeedX, -acceleration, acceleration);
                sign = -1;
            }
            else if(GetKeyState(ConsoleKey.D).Held)
            {
                playerSpeedX += acceleration;
                playerSpeedX = ClampF(playerSpeedX, -acceleration, acceleration);
                sign = 1;

            }
            else if(!(GetKeyState(ConsoleKey.A).Held) &&!(GetKeyState(ConsoleKey.D).Held))
            {
                playerSpeedX -= playerSpeedX / 2;
                playerSpeedX = ClampF(playerSpeedX, -acceleration, acceleration);
                sign = 0;
            }

            xPosition += playerSpeedX;

            if(xPosition < 0) xPosition = 0;
            if (xPosition > 300) xPosition = 300;
            #endregion

            #region vertical movement
            #region gravity
            //get bottom left koordinate of player-rect
            int bottomleft_x = (int)xPosition;
            int bottomright_x = (int)xPosition + outputSprite.Width;
            int bottom_y = (int)yPosition + outputSprite.Height + 1;

            if ( (gameConsole.GetColor(bottomleft_x, bottom_y) != (short)GameConsole.COLOR.BG_DARK_GREEN && gameConsole.GetColor(bottomright_x, bottom_y) != (short)GameConsole.COLOR.BG_DARK_GREEN))
            {
                playerSpeedY += gravity_acceleration;
                playerSpeedY = ClampF(playerSpeedY, -acceleration, acceleration);
            }
            else
            {
                playerSpeedY = 0.0;
                airjumpused = false;
            }
            #endregion

            if (GetKeyState(ConsoleKey.Spacebar).Pressed)
            {
                if (gameConsole.GetColor(bottomleft_x, bottom_y) == (short)GameConsole.COLOR.BG_DARK_GREEN || gameConsole.GetColor(bottomright_x, bottom_y) == (short)GameConsole.COLOR.BG_DARK_GREEN)
                    playerSpeedY = -40;
                else if(!airjumpused)
                {
                    airjumpused = true;
                    playerSpeedY = -40;
                }
            }

            yPosition += playerSpeedY;
            #endregion
        }

        public void BuildSprite()
        {
            if (sign == 0)
                outputSprite = spriteSheet.ReturnPartialSprite(0,0,16,16);
            else if(sign == 1)
            {
                outputSprite = walkingAnimation.outputSprite;
            }
            else if(sign == -1)
            {
                outputSprite = walkingAnimation.outputSprite.FlipHorizontally();
            }
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
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

            using (var f = new JumpAndRun())
                f.Start();
        }
    }
}

