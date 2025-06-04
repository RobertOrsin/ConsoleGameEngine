using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BigGustave;
using ConsoleGameEngine;
using static ConsoleGameEngine.TextWriter;

namespace ConsoleGameEngine
{
    public static class TextWriter
    {
        static Sprite spriteSheet, spriteSheetSMALL;
        public static FontPadding fontPadding = new FontPadding(0, 0, 0, 0);
        static int width, height, widthSMALL, heightSMALL;
        static Dictionary<char, Coords> dictionary = new Dictionary<char, Coords> { { ' ', new Coords {x= 0, y=0 } },
                                                                                    { '!', new Coords {x= 1, y=0 } },
                                                                                    { '"', new Coords {x= 2, y=0 } },
                                                                                    { '#', new Coords {x= 3, y=0 } },
                                                                                    { '$', new Coords {x= 4, y=0 } },
                                                                                    { '%', new Coords {x= 5, y=0 } },
                                                                                    { '&', new Coords {x= 6, y=0 } },
                                                                                    { '\'', new Coords {x= 7, y=0 } },
                                                                                    { '(', new Coords {x= 8, y=0 } },
                                                                                    { ')', new Coords {x= 9, y=0 } },
                                                                                    { '*', new Coords {x= 10, y=0 } },
                                                                                    { '+', new Coords {x= 11, y=0 } },
                                                                                    { ',', new Coords {x= 12, y=0 } },
                                                                                    { '-', new Coords {x= 13, y=0 } },
                                                                                    { '.', new Coords {x= 14, y=0 } },
                                                                                    { '/', new Coords {x= 15, y=0 } },
                                                                                    { '0', new Coords {x= 16, y=0 } },
                                                                                    { '1', new Coords {x= 17, y=0 } },
                                                                                    { '2', new Coords {x= 0, y=1 } },
                                                                                    { '3', new Coords {x= 1, y=1 } },
                                                                                    { '4', new Coords {x= 2, y=1 } },
                                                                                    { '5', new Coords {x= 3, y=1 } },
                                                                                    { '6', new Coords {x= 4, y=1 } },
                                                                                    { '7', new Coords {x= 5, y=1 } },
                                                                                    { '8', new Coords {x= 6, y=1 } },
                                                                                    { '9', new Coords {x= 7, y=1 } },
                                                                                    { ':', new Coords {x= 8, y=1 } },
                                                                                    { ';', new Coords {x= 9, y=1 } },
                                                                                    { '<', new Coords {x= 10, y=1 } },
                                                                                    { '=', new Coords {x= 11, y=1 } },
                                                                                    { '>', new Coords {x= 12, y=1 } },
                                                                                    { '?', new Coords {x= 13, y=1 } },
                                                                                    { '@', new Coords {x= 14, y=1 } },
                                                                                    { 'A', new Coords {x= 15, y=1 } },
                                                                                    { 'B', new Coords {x= 16, y=1 } },
                                                                                    { 'C', new Coords {x= 17, y=1 } },
                                                                                    { 'D', new Coords {x= 0, y=2 } },
                                                                                    { 'E', new Coords {x= 1, y=2 } },
                                                                                    { 'F', new Coords {x= 2, y=2 } },
                                                                                    { 'G', new Coords {x= 3, y=2 } },
                                                                                    { 'H', new Coords {x= 4, y=2 } },
                                                                                    { 'I', new Coords {x= 5, y=2 } },
                                                                                    { 'J', new Coords {x= 6, y=2 } },
                                                                                    { 'K', new Coords {x= 7, y=2 } },
                                                                                    { 'L', new Coords {x= 8, y=2 } },
                                                                                    { 'M', new Coords {x= 9, y=2 } },
                                                                                    { 'N', new Coords {x= 10, y=2 } },
                                                                                    { 'O', new Coords {x= 11, y=2 } },
                                                                                    { 'P', new Coords {x= 12, y=2 } },
                                                                                    { 'Q', new Coords {x= 13, y=2 } },
                                                                                    { 'R', new Coords {x= 14, y=2 } },
                                                                                    { 'S', new Coords {x= 15, y=2 } },
                                                                                    { 'T', new Coords {x= 16, y=2 } },
                                                                                    { 'U', new Coords {x= 17, y=2 } },
                                                                                    { 'V', new Coords {x= 0, y=3 } },
                                                                                    { 'W', new Coords {x= 1, y=3 } },
                                                                                    { 'X', new Coords {x= 2, y=3 } },
                                                                                    { 'Y', new Coords {x= 3, y=3 } },
                                                                                    { 'Z', new Coords {x= 4, y=3 } },
                                                                                    { '[', new Coords {x= 5, y=3 } },
                                                                                    { '\\', new Coords {x= 6, y=3 } },
                                                                                    { ']', new Coords {x= 7, y=3 } },
                                                                                    { '^', new Coords {x= 8, y=3 } },
                                                                                    { '_', new Coords {x= 9, y=3 } },
                                                                                    { '´', new Coords {x= 10, y=3 } },
                                                                                    { 'a', new Coords {x= 11, y=3 } },
                                                                                    { 'b', new Coords {x= 12, y=3 } },
                                                                                    { 'c', new Coords {x= 13, y=3 } },
                                                                                    { 'd', new Coords {x= 14, y=3 } },
                                                                                    { 'e', new Coords {x= 15, y=3 } },
                                                                                    { 'f', new Coords {x= 16, y=3 } },
                                                                                    { 'g', new Coords {x= 17, y=3 } },
                                                                                    { 'h', new Coords {x= 0, y=4 } },
                                                                                    { 'i', new Coords {x= 1, y=4 } },
                                                                                    { 'j', new Coords {x= 2, y=4 } },
                                                                                    { 'k', new Coords {x= 3, y=4 } },
                                                                                    { 'l', new Coords {x= 4, y=4 } },
                                                                                    { 'm', new Coords {x= 5, y=4 } },
                                                                                    { 'n', new Coords {x= 6, y=4 } },
                                                                                    { 'o', new Coords {x= 7, y=4 } },
                                                                                    { 'p', new Coords {x= 8, y=4 } },
                                                                                    { 'q', new Coords {x= 9, y=4 } },
                                                                                    { 'r', new Coords {x= 10, y=4 } },
                                                                                    { 's', new Coords {x= 11, y=4 } },
                                                                                    { 't', new Coords {x= 12, y=4 } },
                                                                                    { 'u', new Coords {x= 13, y=4 } },
                                                                                    { 'v', new Coords {x= 14, y=4 } },
                                                                                    { 'w', new Coords {x= 15, y=4 } },
                                                                                    { 'x', new Coords {x= 16, y=4 } },
                                                                                    { 'y', new Coords {x= 17, y=4 } },
                                                                                    { 'z', new Coords {x= 0, y=5 } },
                                                                                    { '{', new Coords {x= 1, y=5 } },
                                                                                    { '|', new Coords {x= 2, y=5 } },
                                                                                    { '}', new Coords {x= 3, y=5 } },
                                                                                    { '~', new Coords {x= 4, y=5 } }};
        static Dictionary<char, Coords> dictionarySMALL = new Dictionary<char, Coords> { {'A', new Coords {x= 0, y=0 }},
                {'B', new Coords {x= 1, y=0 } },
                {'C', new Coords {x= 2, y=0 } },
                {'D', new Coords {x= 3, y=0 } },
                {'E', new Coords {x= 4, y=0 } },
                {'F', new Coords {x= 5, y=0 } },
                {'G', new Coords {x= 6, y=0 } },
                {'H', new Coords {x= 7, y=0 } },
                {'I', new Coords {x= 8, y=0 } },
                {'J', new Coords {x= 9, y=0 } },
                {'K', new Coords {x= 10, y=0 } },
                {'L', new Coords {x= 11, y=0 } },
                {'M', new Coords {x= 12, y=0 } },
                {'N', new Coords {x= 0, y=1 } },
                {'O', new Coords {x= 1, y=1 } },
                {'P', new Coords {x= 2, y=1 } },
                {'Q', new Coords {x= 3, y=1 } },
                {'R', new Coords {x= 4, y=1 }},
                {'S', new Coords {x= 5, y=1 }},
                {'T', new Coords {x= 6, y=1 }},
                {'U', new Coords {x= 7, y=1 }},
                {'V', new Coords {x= 8, y=1 }},
                {'W', new Coords {x= 9, y=1 }},
                {'X', new Coords {x= 10, y=1 }},
                {'Y', new Coords {x= 11, y=1 }},
                {'Z', new Coords {x= 12, y=1 }},
                {'a', new Coords {x= 0, y=2 }},
                {'b', new Coords {x= 1, y=2 }},
                {'c', new Coords {x= 2, y=2 }},
                {'d', new Coords {x= 3, y=2 }},
                {'e', new Coords {x= 4, y=2 }},
                {'f', new Coords {x= 5, y=2 }},
                {'g', new Coords {x= 6, y=2 }},
                {'h', new Coords {x= 7, y=2 }},
                {'i', new Coords {x= 8, y=2 }},
                {'j', new Coords {x= 9, y=2 }},
                {'k', new Coords {x= 10, y=2 }},
                {'l', new Coords {x= 11, y=2 }},
                {'m', new Coords {x= 12, y=2 }},
                {'n', new Coords {x= 0, y=3 }},
                {'o', new Coords {x= 1, y=3 }},
                {'p', new Coords {x= 2, y=3 }},
                {'q', new Coords {x= 3, y=3 }},
                {'r', new Coords {x= 4, y=3 }},
                {'s', new Coords {x= 5, y=3 }},
                {'t', new Coords {x= 6, y=3 }},
                {'u', new Coords {x= 7, y=3 }},
                {'v', new Coords {x= 8, y=3 }},
                {'w', new Coords {x= 9, y=3 }},
                {'x', new Coords {x= 10, y=3 }},
                {'y', new Coords {x= 11, y=3 }},
                {'z', new Coords {x= 12, y=3 }},

                {'.', new Coords {x= 0, y=4 }},
                {',', new Coords {x= 1, y=4 }},
                {'!', new Coords {x= 2, y=4 }},
                {'?', new Coords {x= 3, y=4 }},
                {':', new Coords {x= 4, y=4 }},
                {';', new Coords {x= 5, y=4 }},
                {'(', new Coords {x= 6, y=4 }},
                {')', new Coords {x= 7, y=4 }},
                {'/', new Coords {x= 8, y=4 }},
                {'+', new Coords {x= 9, y=4 }},
                {'-', new Coords {x= 10, y=4 }},
                {' ', new Coords {x= 11, y=4 }},
                {'_', new Coords {x= 12, y=4 }},
                };

        public static void LoadFont(string fileName, int w, int h)
        {
            spriteSheet = new Sprite(fileName);
            width = w;
            height = h;
        }
        public static void LoadSmallFont(string fileName, int w, int h)
        {
            spriteSheetSMALL = new Sprite(fileName);
            widthSMALL = w;
            heightSMALL = h;
        }
        public static void LoadFont(string fileName, int w, int h, FontPadding _fontPadding)
        {
            spriteSheet = new Sprite(fileName);
            width = w; height = h;
            fontPadding = _fontPadding;
        }
        public static void SetDictionary(Dictionary<char, Coords> _dictionary)
        {
            dictionary = _dictionary;
        }

        public static Sprite GenerateTextSprite(string text, Textalignment textalignment, int fontSize, short backgroundColor = (short)GameConsole.COLOR.FG_WHITE, short foregroundColor = (short)GameConsole.COLOR.FG_BLACK, FontType fontType = FontType.standard)
        {
            Sprite sprite;

            int numberOfLines = text.Split('\n').Count();
            int longesLineLength = text.Split('\n').OrderByDescending(s => s.Length).First().Length;

            int _width = 0, _height = 0;
            Dictionary<char, Coords> _dictonary = new Dictionary<char, Coords>();

            if (fontType == FontType.standard)
            {
                _width = width;
                _height = height;
                _dictonary = dictionary;
            }
            else if (fontType == FontType.small)
            {
                _width = widthSMALL;
                _height = heightSMALL;
                _dictonary = dictionarySMALL;
            }

            sprite = new Sprite(width * fontSize * longesLineLength, height * numberOfLines * fontSize);

            int row = 0;
            foreach (string str in text.Split('\n'))
            {
                int allignmentOffset = 0;
                //offsets for right and center
                if (textalignment == Textalignment.Center)
                {
                    allignmentOffset = (longesLineLength - str.Length) * width * fontSize / 2;
                }
                else if (textalignment == Textalignment.Right)
                {
                    allignmentOffset = (longesLineLength - str.Length) * width * fontSize;
                }

                for (int i = 0; i < str.Length; i += 1)
                {
                   
                    Coords coords = _dictonary[str[i]];
                    Sprite letter = new Sprite(1,1);


                    if (fontType == FontType.small)
                        letter = spriteSheetSMALL.ReturnPartialSprite(coords.x * _width + (fontPadding.left + (coords.x + 1)), coords.y * _height + (fontPadding.top + (coords.y + 1)), _width, _height);
                    else if (fontType == FontType.standard)
                        letter = spriteSheet.ReturnPartialSprite(coords.x * _width + (fontPadding.left + (coords.x + 1)), coords.y * _height + (fontPadding.top + (coords.y + 1)), _width, _height);

                    for (int x = 0; x < letter.Width; x++)
                    {
                        for (int y = 0; y < letter.Height; y++)
                        {
                            short spritesColor = letter.GetColor(x, y);
                            char spritesChar = '\0';
                            if (spritesColor == (short)GameConsole.COLOR.FG_BLACK)
                            {
                                if (foregroundColor != (short)GameConsole.COLOR.TRANSPARENT)
                                    spritesChar = letter.GetChar(x, y);
                                spritesColor = foregroundColor;
                            }
                            else if (spritesColor == (short)GameConsole.COLOR.FG_GREY)
                            {
                                if (backgroundColor != (short)GameConsole.COLOR.TRANSPARENT)
                                    spritesChar = letter.GetChar(x, y);
                                spritesColor = backgroundColor;
                            }

                            sprite.SetBlock(i * _width * fontSize + x*fontSize + allignmentOffset,row*_height*fontSize + y *fontSize, fontSize, fontSize, spritesChar, spritesColor);
                        }
                    }
                }
                row++;
            }
            
            
            
            
            return sprite;
        }
        
        public struct Coords
        {
            public int x { get; set; }
            public int y { get; set; }
        }

        public enum Textalignment
        {
            Left = 0,
            Center = 1,
            Right = 2,
        }

        public enum FontType
        {
            standard,
            small
        }

        public class FontPadding
        {
            public int top, left, right, bottom;

            public FontPadding(int top, int left, int right, int bottom)
            {
                this.top = top;
                this.left = left;
                this.right = right;
                this.bottom = bottom;
            }
        }
    }
}
