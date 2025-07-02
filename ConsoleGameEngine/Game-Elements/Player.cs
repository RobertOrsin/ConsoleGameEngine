using System;
using System.IO;
using static ConsoleGameEngine.GameConsole;
using ConsoleGameEngine.Other;


namespace ConsoleGameEngine
{
    public class Player
    {
        public string displayName;
        private Sprite _displayNameSprite;
        public double xPosition, yPosition, xVelocity, yVelocity;
        public Sprite outputSprite;
        private Sprite spriteSheet;
        private Animation walkingAnimation;

        public int updatedID;

        private bool airjumpused = false;

        private double velocityMax = 30;

        private const double walkSpeed = 2, runSpeed = 5, fallSpeed = 10,  acceleration = 0.5, gravity_acceleration = 2.0;
        private double playerSpeedX = 0.0, playerSpeedY = 0.0;

        private int sign = 0;

        public Player(string displayname)
        {
            displayName = displayname;
            _displayNameSprite = Other.TextWriter.GenerateTextSprite(displayName, backgroundColor: (short)COLOR.TRANSPARENT, foregroundColor: (short)COLOR.FG_WHITE, fontType: Other.TextWriter.FontType.smallest);
            outputSprite = new Sprite(8, 8);
            xPosition = 100.0;
            yPosition = 50.0;
        }

        public void LoadAnimation(string file)
        {
            walkingAnimation = new Animation(file, new TimeSpan(0, 0, 0, 0, 100), 16, 16);
            spriteSheet = new Sprite(file);
        }

        public void Update(KeyState[] KeyStates, TimeSpan elapsedTime, GameConsole gameConsole)
        {
            if (walkingAnimation != null)
                walkingAnimation.Update();
            BuildSprite();

            if (KeyStates != null)
            {
                #region reset
                if (KeyStates[((int)ConsoleKey.R)].Pressed)
                {
                    xPosition = 100.0;
                    yPosition = 50.0;
                    playerSpeedX = 0.0;
                    playerSpeedY = 0.0;
                }

                #endregion

                #region horizontal movement
                if (KeyStates[((int)ConsoleKey.A)].Held)
                {
                    playerSpeedX -= acceleration;
                    playerSpeedX = ClampF(playerSpeedX, -acceleration, acceleration);
                    sign = -1;
                }
                else if (KeyStates[((int)ConsoleKey.D)].Held)
                {
                    playerSpeedX += acceleration;
                    playerSpeedX = ClampF(playerSpeedX, -acceleration, acceleration);
                    sign = 1;

                }
                else if (!(KeyStates[((int)ConsoleKey.A)].Held) && !(KeyStates[((int)ConsoleKey.D)].Held))
                {
                    playerSpeedX -= playerSpeedX / 2;
                    playerSpeedX = ClampF(playerSpeedX, -acceleration, acceleration);
                    sign = 0;
                }

                xPosition += playerSpeedX;

                if (xPosition < 0) xPosition = 0;
                if (xPosition > 300) xPosition = 300;
                #endregion
                //get bottom left koordinate of player-rect
                int bottomleft_x = (int)xPosition;
                int bottomright_x = (int)xPosition + outputSprite.Width;
                int bottom_y = (int)yPosition + outputSprite.Height + 1;

                #region gravity
                if ((gameConsole.GetColor(bottomleft_x, bottom_y) != (short)0x00AA && gameConsole.GetColor(bottomright_x, bottom_y) != (short)0x00AA))
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

                if (KeyStates[((int)ConsoleKey.Spacebar)].Pressed)
                {
                    if (gameConsole.GetColor(bottomleft_x, bottom_y) == (short)0x00AA || gameConsole.GetColor(bottomright_x, bottom_y) == (short)0x00AA)
                        playerSpeedY = -40;
                    else if (!airjumpused)
                    {
                        airjumpused = true;
                        playerSpeedY = -40;
                    }
                }
                yPosition += playerSpeedY;
            }
        }

        public void BuildSprite()
        {
            //width is width of textblock or at least 16
            //height is textblock.height + 3 pixels space + 16 for the sprite
            outputSprite = new Sprite(_displayNameSprite.Width > 16 ? _displayNameSprite.Width : 16, _displayNameSprite.Height + 3 + 16);

            outputSprite.AddSpriteToSprite((outputSprite.Width - _displayNameSprite.Width) / 2, 0, _displayNameSprite);

            var playerspriteY = _displayNameSprite.Height + 3;
            var playerSpriteX = (outputSprite.Width - 16) / 2;


            if (spriteSheet != null)
            {
                if (sign == 0)
                    outputSprite.AddSpriteToSprite(playerSpriteX, playerspriteY, spriteSheet.ReturnPartialSprite(64, 0, 16, 16));
                else if (sign == 1)
                {
                    outputSprite.AddSpriteToSprite(playerSpriteX, playerspriteY, walkingAnimation.outputSprite);
                }
                else if (sign == -1)
                {
                    outputSprite.AddSpriteToSprite(playerSpriteX, playerspriteY, walkingAnimation.outputSprite.FlipHorizontally());
                }
            }
        }
    }
}

