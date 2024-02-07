using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConsoleGameEngine.GameConsole;
using static ConsoleGameEngine.NativeMethods;

namespace ConsoleGameEngine
{
    public class Button
    {
        public int x, y, width, height;
        public Sprite outputSprite, sprite, feedbackSprite;
        string text;
        Func<bool> method;
        bool simple = false;
        short foregroundColor, backgroundColor;

        public Button(int x, int y, int width, int height, Sprite sprite, Sprite feedbackSprite = null)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.sprite = sprite;
            outputSprite = this.sprite;
            this.feedbackSprite = feedbackSprite;
        }
        public Button(int x, int y, Sprite sprite, Sprite feedbackSprite = null)
        {
            this.x = x;
            this.y = y;
            this.sprite = sprite;
            width = sprite.Width;
            height = sprite.Height;
            outputSprite = this.sprite;
            this.feedbackSprite = feedbackSprite;
        }
        public Button(int x, int y, string text, short backgroundColor = (short)COLOR.FG_BLACK, short foregroundColor = (short)COLOR.FG_WHITE)
        {
            this.x = x;
            this.y = y;
            this.text = text;
            width = text.Length + 2;
            height = 3;
            outputSprite = BuildSimpleSprite(false);
            simple = true;
            this.foregroundColor = foregroundColor;
            this.backgroundColor = backgroundColor;
        }

        public void OnButtonClicked(Func<bool> method)
        {
            this.method = method;
        }

        public void Update(MOUSE_EVENT_RECORD r)
        {
            int mouseX = r.dwMousePosition.X, mouseY = r.dwMousePosition.Y;
            uint mouseState = r.dwButtonState;

            if(mouseState == MOUSE_EVENT_RECORD.FROM_LEFT_1ST_BUTTON_PRESSED)
            {
                if(mouseX <= x + width && mouseX >= x && mouseY <= y + height && mouseY > y) 
                {
                    if (!simple)
                    {
                        if (feedbackSprite != null)
                            outputSprite = feedbackSprite;
                    }
                    else
                        outputSprite = BuildSimpleSprite(true);

                    method();
                }
            }
            else
            {
                if(!simple)
                    outputSprite = sprite;
                else
                    outputSprite = BuildSimpleSprite(false);
            }      
        }

        private Sprite BuildSimpleSprite(bool clicked)
        {
            Sprite retSprite = new Sprite(text.Length + 2, 3);

            short color = clicked ? (short)((foregroundColor << 4) + backgroundColor) : (short)((backgroundColor << 4) + foregroundColor);

            for (int i = 1; i < retSprite.Width - 1; i++)
            {
                retSprite.SetPixel(i, 0, '-', color);
                retSprite.SetPixel(i, retSprite.Height - 1, '-', color);
                for (int j = 1; j < retSprite.Height - 1; j++)
                {
                    retSprite.SetPixel(0, j, '|', color);
                    retSprite.SetPixel(retSprite.Width - 1, j, '|', color);
                }
            }
            //corners
            retSprite.SetPixel(0, 0, '+', color);
            retSprite.SetPixel(0, retSprite.Height - 1, '+', color);
            retSprite.SetPixel(retSprite.Width - 1, 0, '+', color);
            retSprite.SetPixel(retSprite.Width - 1, retSprite.Height - 1, '+', color);

            //text
            for(int i = 0; i < text.Length; i++)
            {
                retSprite.SetPixel(1+i,1, text[i], color);
            }


            return retSprite;
        }
    }
}
