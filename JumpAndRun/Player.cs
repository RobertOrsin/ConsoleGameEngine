using System;
using ConsoleGameEngine;
using static ConsoleGameEngine.GameConsole;


namespace JumpAndRun
{
    class Player
    {
        public double xPosition, yPosition, xVelocity, yVelocity;
        public Sprite outputSprite;
        private Sprite spriteSheet;
        private Animation walkingAnimation;

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
            walkingAnimation = new Animation("running ninja.txt", new TimeSpan(0, 0, 0, 0, 100), 16, 16);
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
}

