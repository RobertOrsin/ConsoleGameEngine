using BigGustave;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using static ConsoleGameEngine.GameConsole;

namespace ConsoleGameEngine
{
    public class Sprite
    {
        private Plane<char> _spritedata;
        private Plane<short> _spritecolors;
        private int _width;
        private int _height;

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public Sprite(string[] spriteData, short[] spriteColors = null)
        {
            if (spriteData.Length == 0)
                throw new ArgumentException(nameof(spriteData));
            if (spriteData.Any(s => s.Length != spriteData[0].Length))
                throw new ArgumentException(nameof(spriteData));
            _width = spriteData[0].Length;
            _height = spriteData.Length;


            if (spriteColors != null && spriteColors.Length != _width * _height)
                throw new ArgumentException(nameof(spriteColors));

            _spritedata = new Plane<char>(_width, _height);
            _spritecolors = new Plane<short>(_width, _height);

            int i = 0;

            foreach (var s in spriteData)
                foreach (var c in s)
                    _spritedata.SetData(i++, c);

            for (i = 0; i < _spritecolors.Data.Length; i++)
                _spritecolors.SetData(i, spriteColors == null ? (short)GameConsole.COLOR.FG_GREY : spriteColors[i]);
        }

        public Sprite(string[] spriteData, GameConsole.COLOR[] spriteColors = null)
            : this(spriteData, spriteColors?.Select(c => (short)c).ToArray())
        { }

        public Sprite(string file)
        {
            if (!Load(file)) Create(8, 8);
        }

        public Sprite(int w, int h)
        {
            Create(w, h);
        }

        public Sprite(string file, int w, int h, int startRow, int count)
        {
            if(!Load(file, w, h , startRow, count)) Create(8,8);
        }

        public Sprite ReturnPartialSprite(int x, int y, int w, int h)
        {
            Sprite returnSprite = new Sprite(w, h);

            for(int i = x; i < x + w; i++)
            {
                for(int j = y; j < y +h; j++)
                {
                    returnSprite.SetPixel(i - x, j - y, _spritedata.GetData(i, j), _spritecolors.GetData(i, j));
                }
            }
            return returnSprite;
        }

        #region setter/getter
        public char GetChar(int x, int y)
        {
            return _spritedata.GetData(x, y);
        }
        public short GetColor(int x, int y)
        {
            return _spritecolors.GetData(x, y);
        }

        public void SetChar(int x, int y, char c)
        {
            _spritedata.SetData(x, y, c);
        }

        public void SetColor(int x, int y, short c)
        {
            _spritecolors.SetData(x, y, c);
        }

        public void SetPixel(int x, int y, char c, short col)
        {
            _spritedata.SetData(x, y, c);
            _spritecolors.SetData(x, y, col);
        }

        public void SetBlock(int x, int y, int w, int h, char c, short col)
        {
            for(int i = 0; i < w; i++)
            {
                for(int j = 0; j < h; j++)
                {
                    SetPixel(x + i, y + j, c, col);
                }
            }
        }
        #endregion

        #region load/save/create
        public bool Load(string file)
        {
            _width = 0; _height = 0;

            using (var sr = new StreamReader(file))
            {
                string content = sr.ReadToEnd();

                string[] splits = content.Split(';');

                _width = int.Parse(splits[0]);
                _height = int.Parse(splits[1]);

                _spritedata = new Plane<char>(_width, _height);
                _spritecolors = new Plane<short>(_width, _height);

                int i = 0;
                foreach(string pixel in splits[2].Split(','))
                {
                    if(pixel != "")
                        _spritedata.SetData(i, pixel[0]);
                    i++;
                }
                i = 0;
                foreach (string pixel in splits[3].Split(','))
                {
                    if(pixel != "")
                        _spritecolors.SetData(i, Convert.ToByte(pixel));
                    i++;
                }

            }
            return true;
        }

        public bool Load(string file, int w, int h, int startRow, int count)
        {
            _width = w; _height = h;

            using (var sr = new StreamReader(file))
            {
                string content = sr.ReadToEnd();

                string[] splits = content.Split(';');

                _width = int.Parse(splits[0]);

                _spritedata = new Plane<char>(_width, _height);
                _spritecolors = new Plane<short>(_width, _height);

                int i = 0;

                var pixelSplits = splits[2].Split(',');
                var colorSplits = splits[3].Split(',');

                for (int j = startRow * w; j < (startRow + h) * w; j++)
                {
                    if (pixelSplits[j] != "")
                    {
                        _spritedata.SetData(i, pixelSplits[j][0]);
                        _spritecolors.SetData(i, Convert.ToByte(colorSplits[j]));
                    }
                    i++;
                }
            }
            return true;
        }
        public bool Save(string file)
        {
            System.IO.BinaryWriter sw = null;
            sw = new System.IO.BinaryWriter(File.Open(file, FileMode.OpenOrCreate));
            sw.Write(_width);
            sw.Write(_height);
            foreach (char c in _spritedata.Data)
                sw.Write(c);
            foreach (short s in _spritecolors.Data)
                sw.Write(s);
            sw.Close();

            return true;
        }
        private void Create(int w, int h)
        {
            _width = w;
            _height = h;

            _spritedata = new Plane<char>(_width, _height);
            _spritecolors = new Plane<short>(_width, _height);

            for (int i = 0; i < _width * _height; i++)
                _spritedata.SetData(i, ' ');//m_Glyphs.Add(br.ReadChar());
            for (int i = 0; i < _height * _width; i++)
                _spritecolors.SetData(i, (short)GameConsole.COLOR.BG_BLACK);//m_Colours.Add(br.ReadSByte());
        }
        #endregion


        #region sampling
        public char SampleGlyph(double x, double y)
        {
            int sx = (int)(x * (double)_width);
            int sy = (int)(y * (double)_height - 1.0);

            if (sx < 0 || sx >= _width || sy < 0 || sy >= _height)
                return ' ';
            else
                return _spritedata.GetData(sy * _width + sx);
        }
        public short SampleColor(double x, double y)
        {
            int sx = (int)(x * (double)_width);
            int sy = (int)(y * (double)_height - 1.0);

            if (sx < 0 || sx >= _width || sy < 0 || sy >= _height)
                return (short)GameConsole.COLOR.BG_BLACK;
            else
                return _spritecolors.GetData(sy * _width + sx);
        }
        #endregion
    }
}
