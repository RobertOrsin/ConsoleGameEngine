using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConsoleGameEngine.GameConsole;
using static ConsoleGameEngine.TextBox;

namespace ConsoleGameEngine
{
    public class TextBlock
    {
        public int x, y;
        public Sprite outputSprite;
        int length; //character-count

        public string content = "";
        string tag;
        bool simple = true; // simple - ascii-charcters, advanced - sprites
        short foregroundColor, backgroundColor;
        public int width, height;
        ObjectPosition tagPosition;

        public TextBlock(int x, int y, int length, string tag, bool simple = true, ObjectPosition tagPosition = ObjectPosition.Top, short backgroundColor = (short)COLOR.FG_BLACK, short foregroundColor = (short)COLOR.FG_WHITE, string content = "")
        {
            this.x = x;
            this.y = y;
            this.length = length;
            this.content = content;
            this.tag = tag;
            this.simple = simple;
            this.foregroundColor = foregroundColor;
            this.backgroundColor = backgroundColor;
            this.tagPosition = tagPosition;

            outputSprite = BuildSprite();
        }

        private Sprite BuildSprite()
        {
            //input body
            Sprite body;

            content.PadLeft(length);

            if (simple)
            {
                short color = (short)((backgroundColor << 4) + foregroundColor);

                switch (tagPosition)
                {
                    case ObjectPosition.Top:

                    case ObjectPosition.Bottom:

                    case ObjectPosition.Left:

                    case ObjectPosition.Right: break;
                }

                body = new Sprite(length + 2, 4); //length of input + 2 for frame; height for tag, frame and content
                //frame
                for (int i = 1; i < body.Width - 1; i++)
                {
                    body.SetPixel(i, 1, (char)PIXELS.LINE_STRAIGHT_HORIZONTAL, color);
                    body.SetPixel(i, body.Height - 1, (char)PIXELS.LINE_STRAIGHT_HORIZONTAL, color);
                    for (int j = 1; j < body.Height - 1; j++)
                    {
                        body.SetPixel(0, j, (char)PIXELS.LINE_STRAIGHT_VERTICAL, color);
                        body.SetPixel(body.Width - 1, j, (char)PIXELS.LINE_STRAIGHT_VERTICAL, color);
                    }
                }
                //corners
                body.SetPixel(0, 1, (char)PIXELS.LINE_CORNER_TOP_LEFT, color);
                body.SetPixel(0, body.Height - 1, (char)PIXELS.LINE_CORNER_BOTTOM_LEFT, color);
                body.SetPixel(body.Width - 1, 1, (char)PIXELS.LINE_CORNER_TOP_RIGHT, color);
                body.SetPixel(body.Width - 1, body.Height, (char)PIXELS.LINE_CORNER_BOTTOM_RIGHT, color);

                for (int i = 0; i < content.Length; i++)
                    body.SetPixel(i + 1, 2, content[i], color);

                for (int i = 0; i < tag.Length; i++)
                    body.SetPixel(i, 0, tag[i], color);
            }
            else
            {
                Sprite contentSprite;

                contentSprite = TextWriter.GenerateTextSprite(content, TextWriter.Textalignment.Right, 1, backgroundColor: foregroundColor, foregroundColor: backgroundColor);

                Sprite tagSprite;
                tagSprite = TextWriter.GenerateTextSprite(tag, TextWriter.Textalignment.Right, 1, backgroundColor: backgroundColor, foregroundColor: foregroundColor);

                body = new Sprite(contentSprite.Width > tagSprite.Width ? contentSprite.Width : tagSprite.Width, contentSprite.Height + tagSprite.Height + 3);
                body.AddSpriteToSprite(1, 1, tagSprite);
                body.AddSpriteToSprite(1, tagSprite.Height + 1, contentSprite);

                //frame
                for (int i = 0; i < body.Width; i++)
                {
                    body.SetPixel(i, 0, '█', (short)foregroundColor);
                    body.SetPixel(i, tagSprite.Height, '█', (short)foregroundColor);
                    body.SetPixel(i, body.Height - 1, '█', (short)foregroundColor);
                    for (int j = 0; j < body.Height; j++)
                    {
                        body.SetPixel(0, j, '█', (short)foregroundColor);
                        body.SetPixel(body.Width - 1, j, '█', (short)foregroundColor);
                    }
                }
            }
            width = body.Width; height = body.Height;
            return body;
        }
    }
}
