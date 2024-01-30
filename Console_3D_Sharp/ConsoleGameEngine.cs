using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Threading;
using System.Windows.Input;


namespace Console_3D_Sharp__
{
    class Sprite
    {
        public int nWidth, nHeight;
        private List<short> m_Colours;
        private List<char> m_Glyphs;

        public Sprite() { }
        public Sprite(int w, int h)
        {
            Create(w, h);
        }
        public Sprite(string file)
        {
            if (!Load(file)) Create(8, 8);
        }

        /// <summary>
        /// Creates a black Sprite with given width and height
        /// </summary>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        private void Create(int w, int h)
        {
            nWidth = w;
            nHeight = h;
            m_Colours = new List<short>();
            m_Glyphs = new List<char>();    

            for(int i = 0; i < nWidth*nHeight; i++) 
            {
                m_Colours.Add((short)COLOUR.BG_BLACK);
                m_Glyphs.Add(' ');
            }
        }

        #region Setter/Getter Colour and Glyph
        public void SetGlyph(int x, int y, char c)
        {
            if (x < 0 || x > nWidth || y < 0 || y > nHeight)
                return;
            else
                m_Glyphs[y * nWidth + x] = c;
        }
        public void SetColour(int x, int y, short c)
        {
            if (x < 0 || x > nWidth || y < 0 || y > nHeight)
                return;
            else
                m_Colours[y * nWidth + x] = c;
        }
        public char GetGlyph(int x, int y)
        {
            if (x < 0 || x > nWidth || y < 0 || y > nHeight)
                return ' ';
            else
                return m_Glyphs[y * nWidth + x];
        }
        public short GetColour(int x, int y)
        {
            if (x < 0 || x > nWidth || y < 0 || y > nHeight)
                return (short)COLOUR.BG_BLACK;
            else
                return m_Colours[y * nWidth + x];
        }
        #endregion

        #region Sampling
        public char SampleGlyph(double x, double y)
        {
            int sx = (int)(x * (double)nWidth);
            int sy = (int)(y * (double)nHeight - 1.0);

            if (sx < 0 || sx >= nWidth || sy < 0 || sy >= nHeight)
                return ' ';
            else
                return m_Glyphs[sy * nWidth + sx];
        }
        public short SampleColour(double x, double y)
        {
            int sx = (int)(x * (double)nWidth);
            int sy = (int)(y * (double)nHeight - 1.0);

            if (sx < 0 || sx >= nWidth || sy < 0 || sy >= nHeight)
                return (short)COLOUR.BG_BLACK;
            else
                return m_Colours[sy * nWidth + sx];
        }
        #endregion

        #region Load/Save
        public bool Save(string filename)
        { 
            System.IO.BinaryWriter sw = null;
            sw = new System.IO.BinaryWriter(File.Open(filename, FileMode.OpenOrCreate));
            sw.Write(nWidth);
            sw.Write(nHeight);
            foreach(char c in m_Glyphs)
                sw.Write(c);
            foreach(short  s in m_Colours)
                sw.Write(s);
            sw.Close();

            return true;
        }

        public bool Load(string filename)
        {
            nWidth = 0; nHeight = 0;
            m_Colours = null;
            m_Glyphs = null;

            System.IO.BinaryReader br = null;
            br = new BinaryReader(File.Open(filename, FileMode.Open));

            nWidth = br.ReadUInt16();
            nHeight = br.ReadUInt16();

            Create(nWidth, nHeight);

            for (int i = 0; i < nWidth * nHeight; i++)
                m_Glyphs.Add(br.ReadChar());
            for (int i = 0; i < nHeight * nWidth; i++)
                m_Colours.Add(br.ReadSByte());

            br.Close();

            return true;
        }
        #endregion
    }

    class ConsoleGameEngine
    {
        protected int m_ScreenWidth, m_ScreenHeight;
        protected int m_MousePosX, m_MousePosY;
        protected CHAR_INFO[] m_bufScreen;
        protected List<KeyState> Keys;
        protected string m_AppName;

        public bool m_bAtomActive;

        public ConsoleGameEngine() 
        {
            m_ScreenHeight = 30;
            m_ScreenWidth = 80;
            m_MousePosX = 0;
            m_MousePosY = 0;

            m_AppName = "Default";

            //init keyboard state old/new
            Keys = new List<KeyState>();
            for(int i = 0; i < 256; i++)
                Keys.Add(new KeyState(i));

        }

        public int ConstructConsole(int width, int height, short fontSize)
        {
            m_ScreenHeight = height;
            m_ScreenWidth = width;

            m_bufScreen = new CHAR_INFO[m_ScreenHeight*m_ScreenWidth];

            Console.SetWindowSize(width, height);

            Console.SetBufferSize(width, height);

            ConsoleHelper.SetCurrentFont("Consolas", fontSize);

            //enable mouse controll
            var handle = NativeMethods.GetStdHandle(NativeMethods.STD_INPUT_HANDLE);
            uint mode = 0;
            NativeMethods.GetConsoleMode(handle, ref mode);
            mode &= ~NativeMethods.ENABLE_QUICK_EDIT_MODE; //disable
            mode |= NativeMethods.ENABLE_WINDOW_INPUT; //enable (if you want)
            mode |= NativeMethods.ENABLE_MOUSE_INPUT; //enable
            NativeMethods.SetConsoleMode(handle, mode);

            return 1;
        }

        #region Drawing-Methods
        public virtual void Draw(int x, int y, short px = 0x2588, COLOUR col = COLOUR.BG_WHITE)
        {
            if (x >= 0 && x < m_ScreenWidth && y >= 0 && y < m_ScreenHeight)
            {
                m_bufScreen[y * m_ScreenWidth + x].pixel_type = ((char)px);
                m_bufScreen[y * m_ScreenWidth + x].colour = col;
            }
        }
        public void Fill(int x1, int y1, int x2, int y2, short px = 0x2588, COLOUR col = COLOUR.BG_WHITE)
        {
            Clip(x1, y1);
            Clip(x2, y2);
            for (int x = x1; x < x2; x++)
                for (int y = y1; y < y2; y++)
                    Draw(x, y, px, col);
        }
        public void DrawString(int x, int y, string str, COLOUR col = COLOUR.BG_WHITE)
        {
            int i = 0;
            foreach(char c in str)
            {
                m_bufScreen[y * m_ScreenWidth + x + i].pixel_type = c;
                m_bufScreen[y * m_ScreenWidth + x + i].colour = col;
                i++;
            }
        }
        public void DrawStringAlpha(int x, int y, string str, COLOUR col = COLOUR.BG_WHITE)
        {
            int i = 0;
            foreach(char c in str)
            {
                if (c == ' ')
                {
                    m_bufScreen[y * m_ScreenWidth + x + i].pixel_type = c;
                    m_bufScreen[y * m_ScreenWidth + x + i].colour = col;
                }
                i++;
            }
        }
        public void Clip(int x, int y)
        {
            if (x < 0) x = 0;
            if (x >= m_ScreenWidth) x = m_ScreenWidth;
            if (y < 0) y = 0;
            if (y >= m_ScreenHeight) y = m_ScreenHeight;
        }
        public void DrawLine(int x1, int y1, int x2, int y2, short pt = 0x2588, COLOUR col = COLOUR.BG_WHITE)
        {
            int x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
            dx = x2 - x1; dy = y2 - y1;
            dx1 = Math.Abs(dx); dy1 = Math.Abs(dy);
            px = 2 * dy1 - dx1; py = 2 * dx1 - dy1;
            if (dy1 <= dx1)
            {
                if (dx >= 0)
                { x = x1; y = y1; xe = x2; }
                else
                { x = x2; y = y2; xe = x1; }

                Draw(x, y, pt, col);

                for (i = 0; x < xe; i++)
                {
                    x = x + 1;
                    if (px < 0)
                        px = px + 2 * dy1;
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) y = y + 1; else y = y - 1;
                        px = px + 2 * (dy1 - dx1);
                    }
                    Draw(x, y, pt, col);
                }
            }
            else
            {
                if (dy >= 0)
                { x = x1; y = y1; ye = y2; }
                else
                { x = x2; y = y2; ye = y1; }

                Draw(x, y, pt, col);

                for (i = 0; y < ye; i++)
                {
                    y = y + 1;
                    if (py <= 0)
                        py = py + 2 * dx1;
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) x = x + 1; else x = x - 1;
                        py = py + 2 * (dx1 - dy1);
                    }
                    Draw(x, y, pt, col);
                }
            }
        }
        public void DrawTriangle(int x1, int y1, int x2, int y2, int x3, int y3, short pt = 0x2588, COLOUR col = COLOUR.BG_WHITE)
        {
            DrawLine(x1, y1, x2, y2, pt, col);
            DrawLine(x2, y2, x3, y3, pt, col);
            DrawLine(x3, y3, x1, y1, pt, col);
        }
        public void FillTriangle(int x1, int y1, int x2, int y2, int x3, int y3, short pt = 0x2588, COLOUR col = COLOUR.BG_WHITE)
        {
            int t1x, t2x, y, minx, maxx, t1xp, t2xp;
            bool changed1 = false;
            bool changed2 = false;
            int signx1, signx2, dx1, dy1, dx2, dy2;
            int e1, e2;
            // Sort vertices
            if (y1 > y2) 
            {
                int t = y1;
                y1 = y2;
                y2 = t;

                t = x1;
                x1 = x2;
                x2 = t;
            }
            if (y1 > y3) 
            {
                int t = y1;
                y1 = y2;
                y2 = t;

                t = x1;
                x1 = x3;
                x3 = t;            
            }
            if (y2 > y3) 
            { 
                int t = y2;
                y2 = y3;
                y3 = t;

                t = x2;
                x2 = x3;
                x3 = t;
            }

            t1x = t2x = x1; y = y1;   // Starting points
            dx1 = (int)(x2 - x1); if (dx1 < 0) { dx1 = -dx1; signx1 = -1; }
            else signx1 = 1;
            dy1 = (int)(y2 - y1);

            dx2 = (int)(x3 - x1); if (dx2 < 0) { dx2 = -dx2; signx2 = -1; }
            else signx2 = 1;
            dy2 = (int)(y3 - y1);

            if (dy1 > dx1)
            {   // swap values
                int t = dx1;
                dx1 = dy1;
                dy1 = t;
                changed1 = true;
            }
            if (dy2 > dx2)
            {   // swap values
                int t = dy2;
                dy2 = dx2;
                dx2 = t;
                changed2 = true;
            }

            e2 = (int)(dx2 >> 1);
            // Flat top, just process the second half
            if (y1 == y2) goto next;
            e1 = (int)(dx1 >> 1);

            for (int i = 0; i < dx1;)
            {
                t1xp = 0; t2xp = 0;
                if (t1x < t2x) { minx = t1x; maxx = t2x; }
                else { minx = t2x; maxx = t1x; }
                // process first line until y value is about to change
                while (i < dx1)
                {
                    i++;
                    e1 += dy1;
                    while (e1 >= dx1)
                    {
                        e1 -= dx1;
                        if (changed1) t1xp = signx1;//t1x += signx1;
                        else goto next1;
                    }
                    if (changed1) break;
                    else t1x += signx1;
                }
            // Move line
            next1:
                // process second line until y value is about to change
                while (true)
                {
                    e2 += dy2;
                    while (e2 >= dx2)
                    {
                        e2 -= dx2;
                        if (changed2) t2xp = signx2;//t2x += signx2;
                        else goto next2;
                    }
                    if (changed2) break;
                    else t2x += signx2;
                }
            next2:
                if (minx > t1x) minx = t1x; if (minx > t2x) minx = t2x;
                if (maxx < t1x) maxx = t1x; if (maxx < t2x) maxx = t2x;

                for (int j = minx; j <= maxx; j++) Draw(j, y, pt, col); // Draw line from min to max points found on the y
                // Now increase y
                if (!changed1) t1x += signx1;
                t1x += t1xp;
                if (!changed2) t2x += signx2;
                t2x += t2xp;
                y += 1;
                if (y == y2) break;

            }
        next:
            // Second half
            dx1 = (int)(x3 - x2); if (dx1 < 0) { dx1 = -dx1; signx1 = -1; }
            else signx1 = 1;
            dy1 = (int)(y3 - y2);
            t1x = x2;

            if (dy1 > dx1)
            {   // swap values
                int t = dy1;
                dy1 = dx1;
                dx1 = t;
                changed1 = true;
            }
            else changed1 = false;

            e1 = (int)(dx1 >> 1);

            for (int i = 0; i <= dx1; i++)
            {
                t1xp = 0; t2xp = 0;
                if (t1x < t2x) { minx = t1x; maxx = t2x; }
                else { minx = t2x; maxx = t1x; }
                // process first line until y value is about to change
                while (i < dx1)
                {
                    e1 += dy1;
                    while (e1 >= dx1)
                    {
                        e1 -= dx1;
                        if (changed1) { t1xp = signx1; break; }//t1x += signx1;
                        else goto next3;
                    }
                    if (changed1) break;
                    else t1x += signx1;
                    if (i < dx1) i++;
                }
            next3:
                // process second line until y value is about to change
                while (t2x != x3)
                {
                    e2 += dy2;
                    while (e2 >= dx2)
                    {
                        e2 -= dx2;
                        if (changed2) t2xp = signx2;
                        else goto next4;
                    }
                    if (changed2) break;
                    else t2x += signx2;
                }
            next4:

                if (minx > t1x) minx = t1x; if (minx > t2x) minx = t2x;
                if (maxx < t1x) maxx = t1x; if (maxx < t2x) maxx = t2x;


                for (int j = minx; j <= maxx; j++) Draw(j, y, pt, col);

                if (!changed1) t1x += signx1;
                t1x += t1xp;
                if (!changed2) t2x += signx2;
                t2x += t2xp;
                y += 1;
                if (y > y3) return;
            }
        }

        public void DrawCircle(int xc, int yc, int r, short pt = 0x2588, COLOUR col = COLOUR.BG_WHITE)
        {
            int x = 0;
            int y = r;
            int p = 3 - 2 * r;
            //if (!r) return;

            while (y >= x) // only formulate 1/8 of circle
            {
                Draw(xc - x, yc - y, pt, col);//upper left left
                Draw(xc - y, yc - x, pt, col);//upper upper left
                Draw(xc + y, yc - x, pt, col);//upper upper right
                Draw(xc + x, yc - y, pt, col);//upper right right
                Draw(xc - x, yc + y, pt, col);//lower left left
                Draw(xc - y, yc + x, pt, col);//lower lower left
                Draw(xc + y, yc + x, pt, col);//lower lower right
                Draw(xc + x, yc + y, pt, col);//lower right right
                if (p < 0) p += 4 * x++ + 6;
                else p += 4 * (x++ - y--) + 10;
            }
        }
        public void FillCircle(int xc, int yc, int r, short pt = 0x2588, COLOUR col = COLOUR.BG_WHITE)
        {
            int x = 0;
            int y = r;
            int p = 3 - 2 * r;
            //if (!r) return;

            void Internaldrawline(int sx, int ex, int ny)
            {
                for (int i = sx; i <= ex; i++)
                    Draw(i, ny, pt, col);
            };

            while (y >= x)
            {
                // Modified to draw scan-lines instead of edges
                Internaldrawline(xc - x, xc + x, yc - y);
                Internaldrawline(xc - y, xc + y, yc - x);
                Internaldrawline(xc - x, xc + x, yc + y);
                Internaldrawline(xc - y, xc + y, yc + x);
                if (p < 0) p += 4 * x++ + 6;
                else p += 4 * (x++ - y--) + 10;
            }
        }

        public void DrawSprite(int x, int y, Sprite sprite)
        {
            if (sprite == null)
                return;
            
            for(int i = 0; i < sprite.nWidth; i++)
            {
                for(int j = 0; j < sprite.nHeight; j++)
                {
                    if (sprite.GetGlyph(i, j) != ' ')
                        Draw(x + i, y + 1, (short)sprite.GetGlyph(i, j), (COLOUR)sprite.GetColour(i, j));
                }
            }
        }
        public void DrawPartialSprite(int x, int y, Sprite sprite, int ox, int oy, int w, int h)
        {
            if (sprite == null)
                return;

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (sprite.GetGlyph(i + ox, j + oy) != ' ')
					    Draw(x + i, y + j, (short)sprite.GetGlyph(i + ox, j + oy), (COLOUR)sprite.GetColour(i + ox, j + oy));
                }
            }
        }

        //void DrawWireFrameModel()...
        #endregion


        #region setter/getter
        public int GetScreenWidth() { return m_ScreenWidth; }
        public int GetScreenHeight() { return m_ScreenHeight; }
        #endregion

        public void Start()
        {
            m_bAtomActive = true;

            Thread t = new Thread(GameThread);

            t.Start();
            t.Join();
        }

        private void GameThread()
        {
            if(!OnUserCreate())
                m_bAtomActive=false;


            var tp1 = DateTime.Now;
            var tp2 = DateTime.Now;

            while(m_bAtomActive)
            {
                while(m_bAtomActive)
                {
                    //Timing
                    tp2 = DateTime.Now;
                    TimeSpan elapsed = tp2 - tp1;
                    tp1 = tp2;
                    float fElapsedTime = (float)elapsed.TotalSeconds;

                    //Keyboard
                    foreach (KeyState keyState in Keys)
                        keyState.UpdateState();


                    if (!OnUserUpdate(fElapsedTime))
                        m_bAtomActive = false;

                    string title = $"3D-GameEngine for Windows Console - {m_AppName} - FPS: {1.0 / fElapsedTime}";
                    Console.Title = title;
                    Console.SetCursorPosition(0, 0);
                    foreach(CHAR_INFO ch in m_bufScreen)
                    {
                        Console.ForegroundColor = (ConsoleColor)ch.colour;
                        Console.Write(ch.pixel_type);
                    }
                    //Console.Write(m_bufScreen);
                    
                }
            }
        }

        public virtual bool OnUserCreate() { return false; }
        public virtual bool OnUserUpdate(double fElapsedTime) { return false; }
    }
    enum COLOUR
    {
        FG_BLACK = 0x0000,
        FG_DARK_BLUE = 0x0001,
        FG_DARK_GREEN = 0x0002,
        FG_DARK_CYAN = 0x0003,
        FG_DARK_RED = 0x0004,
        FG_DARK_MAGENTA = 0x0005,
        FG_DARK_YELLOW = 0x0006,
        FG_GREY = 0x0007, // Thanks MS :-/
        FG_DARK_GREY = 0x0008,
        FG_BLUE = 0x0009,
        FG_GREEN = 0x000A,
        FG_CYAN = 0x000B,
        FG_RED = 0x000C,
        FG_MAGENTA = 0x000D,
        FG_YELLOW = 0x000E,
        FG_WHITE = 0x000F,
        BG_BLACK = 0x0000,




        BG_DARK_BLUE = 0x0010,
        BG_DARK_GREEN = 0x0020,
        BG_DARK_CYAN = 0x0030,
        BG_DARK_RED = 0x0040,
        BG_DARK_MAGENTA = 0x0050,
        BG_DARK_YELLOW = 0x0060,
        BG_GREY = 0x0070,
        BG_DARK_GREY = 0x0080,
        BG_BLUE = 0x0090,
        BG_GREEN = 0x00A0,
        BG_CYAN = 0x00B0,
        BG_RED = 0x00C0,
        BG_MAGENTA = 0x00D0,
        BG_YELLOW = 0x00E0,
        BG_WHITE = 0x00F0,
    };
    enum PIXEL_TYPE
    {
        PIXEL_SOLID = 0x2588,
        PIXEL_THREEQUARTERS = 0x2593,
        PIXEL_HALF = 0x2592,
        PIXEL_QUARTER = 0x2591,
    };
    struct CHAR_INFO
    {
        public char pixel_type;
        public COLOUR colour;
    }

    class KeyState
    {
        private int index;
        public KeyStates oldState;
        public KeyStates newState;
        public bool pressed, released, held;

        public KeyState(int _index)
        {
            this.index = _index;
        }
        public void UpdateState()
        { 
           // newState = Keyboard.GetKeyStates((Key)index);

            pressed = false; released = false;

            if(oldState != newState)
            {
                if(newState == KeyStates.Down)
                {
                    pressed = !held;
                    held = true;
                }
                else
                {
                    released = true;
                    held = false;
                }
            }
            oldState = newState;
        }

    }

    public static class ConsoleHelper
    {
        private const int FixedWidthTrueType = 54;
        private const int StandardOutputHandle = -11;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool MaximumWindow, ref FontInfo ConsoleCurrentFontEx);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool MaximumWindow, ref FontInfo ConsoleCurrentFontEx);


        private static readonly IntPtr ConsoleOutputHandle = GetStdHandle(StandardOutputHandle);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct FontInfo
        {
            internal int cbSize;
            internal int FontIndex;
            internal short FontWidth;
            public short FontSize;
            public int FontFamily;
            public int FontWeight;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            //[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.wc, SizeConst = 32)]
            public string FontName;
        }

        public static FontInfo[] SetCurrentFont(string font, short fontSize = 0)
        {
            Console.WriteLine("Set Current Font: " + font);

            FontInfo before = new FontInfo
            {
                cbSize = Marshal.SizeOf<FontInfo>()
            };

            if (GetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref before))
            {

                FontInfo set = new FontInfo
                {
                    cbSize = Marshal.SizeOf<FontInfo>(),
                    FontIndex = 0,
                    FontFamily = FixedWidthTrueType,
                    FontName = font,
                    FontWeight = 400,
                    FontSize = fontSize > 0 ? fontSize : before.FontSize
                };

                // Get some settings from current font.
                if (!SetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref set))
                {
                    var ex = Marshal.GetLastWin32Error();
                    Console.WriteLine("Set error " + ex);
                    throw new System.ComponentModel.Win32Exception(ex);
                }

                FontInfo after = new FontInfo
                {
                    cbSize = Marshal.SizeOf<FontInfo>()
                };
                GetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref after);

                return new[] { before, set, after };
            }
            else
            {
                var er = Marshal.GetLastWin32Error();
                Console.WriteLine("Get error " + er);
                throw new System.ComponentModel.Win32Exception(er);
            }
        }
    }


}
