using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using static ConsoleGameEngine.GameConsole;
using static ConsoleGameEngine.NativeMethods;

namespace ConsoleGameEngine
{
    public class TextBox
    {
        public int x, y;
        public Sprite outputSprite;
        int length; //character-count

        public string content = "";
        string tag;
        bool selected = false;
        bool simple = true; // simple - ascii-charcters, advanced - sprites
        short foregroundColor, backgroundColor;
        ObjectPosition tagPosition;

        int inputFieldWidth, inputFieldHeight;

        TimeSpan buttonDelay = new TimeSpan();
        TimeSpan buttonTime = new TimeSpan(0, 0, 0, 0, 120);

        public TextBox(int x, int y, int length, string tag, bool simple = true, ObjectPosition tagPosition = ObjectPosition.Top, short backgroundColor = (short)COLOR.FG_BLACK, short foregroundColor = (short)COLOR.FG_WHITE)
        {
            this.x = x;
            this.y = y;
            this.length = length;
            this.tag = tag;
            this.simple = simple;
            outputSprite = new Sprite(1,1);
            this.foregroundColor = foregroundColor;
            this.backgroundColor = backgroundColor;
            this.tagPosition = tagPosition;
        }
        
        public void UpdateSelection(MOUSE_EVENT_RECORD r)
        {
            int mouseX = r.dwMousePosition.X, mouseY = r.dwMousePosition.Y;
            uint mouseState = r.dwButtonState;

            if (mouseState == MOUSE_EVENT_RECORD.FROM_LEFT_1ST_BUTTON_PRESSED)
                if (mouseX <= x + inputFieldWidth && mouseX >= x && mouseY <= y + inputFieldHeight && mouseY > y)
                    selected = true;
                else
                    selected = false;
        }
        public void UpdateInput(KeyState[] KeyStates, TimeSpan elapsedTime)
        {
            buttonDelay += elapsedTime;

            //check for keyboard inputs if selected
            if(selected)
            {
                if (content.Length < length)
                {
                    //A-Z
                    for (int i = 65; i <= 90; i++)
                        if (GetKeyState((ConsoleKey)i).Held && buttonDelay >= buttonTime)
                        {
                            content += Console.CapsLock ? (char)i : (char)(i + 32);
                            buttonDelay = new TimeSpan();
                        }

                    //0 - 9 - ignores capslock
                    for (int i = 48; i <= 57; i++)
                        if (GetKeyState((ConsoleKey)i).Held && buttonDelay >= buttonTime)
                        {
                            content += (char)(i);
                            buttonDelay = new TimeSpan();
                        }

                    //seperators (,.;:-)
                    if (KeyStates[108].Held && buttonDelay >= buttonTime)
                    {
                        content += Console.CapsLock ? ':' : '.';
                        buttonDelay = new TimeSpan();
                    }
                    if (KeyStates[109].Held && buttonDelay >= buttonTime)
                    {
                        content += Console.CapsLock ? '_' : '-';
                        buttonDelay = new TimeSpan();
                    }
                    if (KeyStates[110].Held && buttonDelay >= buttonTime)
                    {
                        content += Console.CapsLock ? ';' : ',';
                        buttonDelay = new TimeSpan();
                    }

                    if (KeyStates[32].Held && buttonDelay >= buttonTime) //space
                    {
                        content += ' ';
                        buttonDelay = new TimeSpan();
                    }
                }

                //(back-)space / enter
                if (KeyStates[13].Held && buttonDelay >= buttonTime) //enter
                    selected = false;
                
                if (KeyStates[8].Held && buttonDelay >= buttonTime) //backspace
                {
                    content = content.Length > 0 ? content.Remove(content.Length - 1) : content;
                    buttonDelay = new TimeSpan();
                }
            }

            //build sprite
            BuildSprite();
        }

        private void BuildSprite()
        {
            //input body
            Sprite body;

            content.PadLeft(length);

            if(simple)
            {
                short color = selected ? (short)((foregroundColor << 4) + backgroundColor) : (short)((backgroundColor << 4) + foregroundColor);

                switch(tagPosition)
                {
                    case ObjectPosition.Top:

                    case ObjectPosition.Bottom:

                    case ObjectPosition.Left:

                    case ObjectPosition.Right: break;
                }
                body = new Sprite(length + 2, 4); //length of input + 2 for frame; height for tag, frame and content
                //frame
                for(int i = 1; i < body.Width - 1; i++)
                {
                    body.SetPixel(i, 1, '-', color);
                    body.SetPixel(i, body.Height - 1, '-', color);
                    for (int j = 1; j < body.Height - 1; j++)
                    {
                        body.SetPixel(0, j, '|', color);
                        body.SetPixel(body.Width - 1, j, '|', color);
                    }
                }
                //corners
                body.SetPixel(0, 1, '+', color);
                body.SetPixel(0, body.Height - 1, '+', color);
                body.SetPixel(body.Width - 1, 1, '+', color);
                body.SetPixel(body.Width - 1, body.Height, '+', color);

                for(int i = 0; i < content.Length; i++)
                    body.SetPixel(i+1,2, content[i], color);

                for (int i = 0; i < tag.Length; i++)
                    body.SetPixel(i, 0, tag[i], color);
            }
            else
            {
                Sprite contentSprite;
                if (!selected)
                    contentSprite = TextWriter.GenerateTextSprite(content, TextWriter.Textalignment.Right, 1, backgroundColor:backgroundColor, foregroundColor:foregroundColor);
                else
                    contentSprite = TextWriter.GenerateTextSprite(content, TextWriter.Textalignment.Right, 1, backgroundColor: foregroundColor, foregroundColor: backgroundColor);

                Sprite tagSprite;
                tagSprite = TextWriter.GenerateTextSprite(tag, TextWriter.Textalignment.Right, 1, backgroundColor: backgroundColor, foregroundColor: foregroundColor);

                body = new Sprite(contentSprite.Width > tagSprite.Width ? contentSprite.Width : tagSprite.Width, contentSprite.Height + tagSprite.Height + 3);
                body.AddSpriteToSprite(1, 1, tagSprite);
                body.AddSpriteToSprite(1, tagSprite.Height + 1, contentSprite);

                //frame
                for(int i = 0; i < body.Width; i++)
                {
                    body.SetPixel(i, 0, '█', selected ? (short)COLOR.FG_RED : (short)foregroundColor);
                    body.SetPixel(i, tagSprite.Height, '█', selected ? (short)COLOR.FG_RED : (short)foregroundColor);
                    body.SetPixel(i, body.Height - 1, '█', selected ? (short)COLOR.FG_RED : (short)foregroundColor);
                    for (int j = 0; j < body.Height; j++)
                    {
                        body.SetPixel(0, j, '█', selected ? (short)COLOR.FG_RED : (short)foregroundColor);
                        body.SetPixel(body.Width - 1, j, '█', selected ? (short)COLOR.FG_RED : (short)foregroundColor);
                    }
                }
            }

            inputFieldWidth = body.Width;
            inputFieldHeight = body.Height;
            outputSprite = body;
        }

        public enum ObjectPosition
        {
            Top, Bottom, Left, Right,
        }
    }
}
