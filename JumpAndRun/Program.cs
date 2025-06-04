using System;
using System.Collections.Generic;
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
        TimeSpan keyInputDelay = new TimeSpan(), keyInputTime = new TimeSpan(0, 0, 0, 0, 120);
        IntPtr inHandle;
        int cursorX = 0, cursorY = 0;
        bool leftMousebuttonClicked = false, mouseWheelClicked = false, rightMousebuttonClicked = false;

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
            player.LoadAnimation("runnin ninja.txt");
            //Load sprites, setup variables and whatever
            return true;
        }
        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            keyInputDelay += elapsedTime;
            player.Update(KeyStates, elapsedTime, this);
            
            Clear();
            DrawSprite((int)player.xPosition, (int)player.yPosition, player.outputSprite);

            DrawSprite(50, 70, new Sprite(70, 1, GameConsole.COLOR.BG_DARK_GREEN));
            DrawSprite(90, 100, new Sprite(70, 1, GameConsole.COLOR.BG_DARK_GREEN));
            DrawSprite(0, 119, new Sprite(199, 1, GameConsole.COLOR.BG_DARK_GREEN));
            DrawSprite(150, 40, new Sprite(35, 1, GameConsole.COLOR.BG_DARK_GREEN));
            DrawSprite(130, 55, new Sprite(20, 1, GameConsole.COLOR.BG_DARK_GREEN));

            DrawSprite(0, 0, TextWriter.GenerateTextSprite($"X: {cursorX}; Y: {cursorY}", TextWriter.Textalignment.Left, 1));
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

            if (gameConsole.GetColor(bottomleft_x, bottom_y) != (short)GameConsole.COLOR.BG_DARK_GREEN && gameConsole.GetColor(bottomright_x, bottom_y) != (short)GameConsole.COLOR.BG_DARK_GREEN)
            {
                playerSpeedY += gravity_acceleration;
                playerSpeedY = ClampF(playerSpeedY, -acceleration, acceleration);
            }
            else
            {
                playerSpeedY = 0.0;
            }
            #endregion

            if (GetKeyState(ConsoleKey.Spacebar).Pressed)
            {
                //disable double jumps
                if (gameConsole.GetColor(bottomleft_x, bottom_y) == (short)GameConsole.COLOR.BG_DARK_GREEN || gameConsole.GetColor(bottomright_x, bottom_y) == (short)GameConsole.COLOR.BG_DARK_GREEN)
                    playerSpeedY = -40;
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

