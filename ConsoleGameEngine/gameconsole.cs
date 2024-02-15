using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using static ConsoleGameEngine.GameConsole;


namespace ConsoleGameEngine
{
    public abstract class GameConsole : IDisposable
    {
        #region Windows API
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "WriteConsoleOutputW")]
        private static extern bool WriteConsoleOutput(
            SafeFileHandle hConsoleOutput,
            CharInfo[] lpBuffer,
            Coord dwBufferSize,
            Coord dwBufferCoord,
            ref SmallRect lpWriteRegion
        );

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            public short X;
            public short Y;

            public Coord(short x, short y)
            {
                X = x;
                Y = y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        private struct CharUnion
        {
            [FieldOffset(0)] public short UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Auto)]
        private struct CharInfo
        {
            [FieldOffset(0)] public CharUnion Char;
            [FieldOffset(2)] public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class CONSOLE_FONT_INFOEX
        {
            private int cbSize;
            public CONSOLE_FONT_INFOEX()
            {
                cbSize = Marshal.SizeOf(typeof(CONSOLE_FONT_INFOEX));
            }

            public int FontIndex;
            public short FontWidth;
            public short FontHeight;
            public int FontFamily;
            public int FontWeight;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string FaceName;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetCurrentConsoleFontEx
          (
          IntPtr ConsoleOutput,
          bool MaximumWindow,
          [In, Out] CONSOLE_FONT_INFOEX ConsoleCurrentFontEx
        );

        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }


        #endregion
        public struct KeyState
        {
            public bool Pressed;
            public bool Released;
            public bool Held;
        }

        private readonly SafeFileHandle _consolehandle;
        private readonly Plane<CharInfo> _screenbuf;
        public readonly Coord _screencoord;
        private readonly Coord _topleft = new Coord() { X = 0, Y = 0 };
        private readonly Thread _gamethread;
        private SmallRect _screenrect;

        private const int KEYSTATES = 0XFF;
        private readonly short[] _newkeystate = new short[KEYSTATES];
        private readonly short[] _oldkeystate = new short[KEYSTATES];

        public static KeyState[] KeyStates { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public string Title { get { return Console.Title; } set { Console.Title = value ?? "GameConsole"; } }

        public enum PIXELS
        {
            PIXEL_NONE = '\0',
            PIXEL_SOLID = 0x2588,
            PIXEL_THREEQUARTERS = 0x2593,
            PIXEL_HALF = 0x2592,
            PIXEL_QUARTER = 0x2591,
            LINE_STRAIGHT_HORIZONTAL = '─',
            LINE_STRAIGHT_VERTICAL = '│',
            LINE_CORNER_TOP_LEFT = '┌',
            LINE_CORNER_TOP_RIGHT = '┐',
            LINE_CORNER_BOTTOM_LEFT = '└',
            LINE_CORNER_BOTTOM_RIGHT = '┘',
            LINE_TSECTION_TOP = '┬',
            LINE_TSECTION_BOTTOM = '┴',
            LINE_TSECTION_LEFT = '├',
            LINE_TSECTION_RIGHT = '┤',
            LINE_CROSSSECTION = '┼',
        }

        public enum COLOR
        {
            FG_BLACK = 0x0000,
            FG_DARK_BLUE = 0x0001,
            FG_DARK_GREEN = 0x0002,
            FG_DARK_CYAN = 0x0003,
            FG_DARK_RED = 0x0004,
            FG_DARK_MAGENTA = 0x0005,
            FG_DARK_YELLOW = 0x0006,
            FG_GREY = 0x0007,
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
            TRANSPARENT = 0x00FF,
        }

        public GameConsole(short width, short height, string title = null, string font = "Consolas", short fontwidth = 8, short fontheight = 8)
        {
            Width = width;
            Height = height;
            Title = title;

            KeyStates = new KeyState[KEYSTATES];

            _consolehandle = CreateFile("CONOUT$", 0x40000000, 0x02, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            if (!_consolehandle.IsInvalid)
            {
                _screenbuf = new Plane<CharInfo>(Width, Height);
                _screenrect = new SmallRect() { Left = 0, Top = 0, Right = (short)Width, Bottom = (short)Height };
                _screencoord = new Coord() { X = (short)Width, Y = (short)Height };

                var cfi = new CONSOLE_FONT_INFOEX()
                {
                    FaceName = font,
                    FontWidth = fontwidth,
                    FontHeight = fontheight,
                    FontFamily = 0,            //FF_DONTCARE
                    FontWeight = 0x0190,       //FW_NORMAL
                    FontIndex = 0,
                };

                SetCurrentConsoleFontEx(_consolehandle.DangerousGetHandle(), false, cfi);
            }

            if (width > Console.LargestWindowWidth || height > Console.LargestWindowHeight)
                throw new InvalidOperationException($"Unable to create console; maximum width/height are {Console.LargestWindowWidth} x {Console.LargestWindowHeight}");

            Console.WindowWidth = width;
            Console.WindowHeight = height;
            Console.CursorVisible = false;
            //Console.OutputEncoding = Encoding.Unicode;
             // Console.OutputEncoding = Encoding.UTF8;

            _gamethread = new Thread(() =>
            {
                if (OnUserCreate())
                {
                    var tp1 = DateTime.Now;
                    var tp2 = DateTime.Now;


                    var sw = Stopwatch.StartNew();
                    var cont = true;

                    while (cont)
                    {
                        GetKeyStates();

                        tp2 = DateTime.Now;
                        TimeSpan elapsed = tp2 - tp1;
                        tp1 = tp2;
                        cont = OnUserUpdate(elapsed);
                        Paint();
                    };

                }
            });
        }

        public void DrawSprite(int x, int y, Sprite sprite, char alphaChar = '\0', short alphaColor = 0x0000)
        {
            for (int py = 0; py < sprite.Height; py++)
            {
                for (int px = 0; px < sprite.Width; px++)
                {
                    var c = sprite.GetChar(px, py);
                    var col = sprite.GetColor(px, py);
                    if (c != alphaChar && col != alphaColor)
                        SetChar(x + px, y + py, sprite.GetChar(px, py), sprite.GetColor(px, py));
                }
            }
        }

        public void DrawPartialSprite(int x, int y, Sprite sprite, int ox, int oy, int w, int h)
        {
            for(int i = 0; i < w; i++)
            {
                for(int j = 0; j < h; j++)
                {
                    SetChar(x + i, y + j, sprite.GetChar(i + ox, j + oy), sprite.GetColor(i+ox, j+oy));
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fWorldX">X for Cameraposition in World</param>
        /// <param name="fWorldY">Y for Cameraposition in World</param>
        /// <param name="fWorldA">A for Cameraposition in World</param>
        /// <param name="fFar">Farthest rendered point</param>
        /// <param name="fNear">Nearest rendered point</param>
        /// <param name="fFoVHalf">Field of view, f.e. "pi / 4"</param>
        /// <param name="sprite">the sprite to be used</param>
        /// <param name="invert">start from bottom or top (ground or sky)</param>
        public void Mode7(double fWorldX, double fWorldY, double fWorldA, double fNear, double fFar, double fFoVHalf, Sprite sprite, bool invert)
        {
            // Create Frustum corner points
            double fFarX1 = fWorldX + Math.Cos(fWorldA - fFoVHalf) * fFar;
            double fFarY1 = fWorldY + Math.Sin(fWorldA - fFoVHalf) * fFar;

            double fNearX1 = fWorldX + Math.Cos(fWorldA - fFoVHalf) * fNear;
            double fNearY1 = fWorldY + Math.Sin(fWorldA - fFoVHalf) * fNear;

            double fFarX2 = fWorldX + Math.Cos(fWorldA + fFoVHalf) * fFar;
            double fFarY2 = fWorldY + Math.Sin(fWorldA + fFoVHalf) * fFar;

            double fNearX2 = fWorldX + Math.Cos(fWorldA + fFoVHalf) * fNear;
            double fNearY2 = fWorldY + Math.Sin(fWorldA + fFoVHalf) * fNear;

            // Starting with furthest away line and work towards the camera point
            for (int y = 0; y < Height / 2; y++)
            {
                // Take a sample point for depth linearly related to rows down screen
                double fSampleDepth = (double)y / Height / 2.0;

                // Use sample point in non-linear (1/x) way to enable perspective
                // and grab start and end points for lines across the screen
                double fStartX = (fFarX1 - fNearX1) / (fSampleDepth) + fNearX1;
                double fStartY = (fFarY1 - fNearY1) / (fSampleDepth) + fNearY1;
                double fEndX = (fFarX2 - fNearX2) / (fSampleDepth) + fNearX2;
                double fEndY = (fFarY2 - fNearY2) / (fSampleDepth) + fNearY2;

                // Linearly interpolate lines across the screen
                for (int x = 0; x < Width; x++)
                {
                    double fSampleWidth = (double)x / Width;
                    double fSampleX = (fEndX - fStartX) * fSampleWidth + fStartX;
                    double fSampleY = (fEndY - fStartY) * fSampleWidth + fStartY;

                    // Wrap sample coordinates to give "infinite" periodicity on maps
                    fSampleX = fSampleX % 1.0; //fmod(fSampleX, 1.0f);
                    fSampleY = fSampleY % 1.0;//fmod(fSampleY, 1.0f);

                    // Sample symbol and colour from map sprite, and draw the
                    // pixel to the screen
                    char sym = sprite.SampleGlyph(fSampleX, fSampleY);
                    short col = sprite.SampleColor(fSampleX, fSampleY);

                    if(!invert)
                        SetChar(x, y + (Height / 2), sym, col);
                    else
                        SetChar(x, (Height / 2) - y, sym, col);
                }
            }
        }

        public void Clear()
        {
            Fill(0, 0, Width, Height, (char)PIXELS.PIXEL_NONE, (short)COLOR.BG_BLACK);
        }

        public void Fill(int x, int y, int width, int height, char c = (char)PIXELS.PIXEL_NONE, short attributes = (short)COLOR.BG_BLACK)
        {
            for (int xp = x; xp < x + width; xp++)
                for (int yp = y; yp < y + height; yp++)
                    SetChar(xp, yp, c, attributes);
            
        }

        public void Print(int x, int y, string text, short attributes = (int)COLOR.FG_WHITE)
        {
            for (int i = 0; i < text.Length; ++i)
            {
                SetChar(x + i, y, text[i], attributes);
            }
        }

        public void DrawRectangle(int x, int y, int width, int height, short color =  (short)COLOR.FG_WHITE)
        {
            short drawingcolor = (short)(color << 4);
            drawingcolor += color;
            for(int i = x; i <= x+width; i++)
            {
                SetChar(i, y, '█', drawingcolor);
                SetChar(i, y + height, '█', drawingcolor);
            }
            for (int j = y; j <= y + height; j++)
            {
                SetChar(x, j, '█', drawingcolor);
                SetChar(x+width, j, '█', drawingcolor);
            }
        }

        public void DrawASCIIRectangle(int x, int y, int width, int height, short background = (short)COLOR.BG_BLACK, short foreground = (short)COLOR.FG_WHITE)
        {
            short drawingColor = (short)(background + foreground);

            for(int i = x; i < x+width; i++)
            {
                SetChar(i, y, (char)PIXELS.LINE_STRAIGHT_HORIZONTAL, drawingColor);
                SetChar(i, y + height - 1, (char)PIXELS.LINE_STRAIGHT_HORIZONTAL, drawingColor);

                for(int j = y; j < y+height; j++)
                {
                    SetChar(x, j, (char)PIXELS.LINE_STRAIGHT_VERTICAL, drawingColor);
                    SetChar(x + width - 1, j, (char)PIXELS.LINE_STRAIGHT_VERTICAL, drawingColor);
                }
            }
            //corners
            SetChar(x,y, (char)PIXELS.LINE_CORNER_TOP_LEFT, drawingColor);
            SetChar(x, y + height - 1, (char)PIXELS.LINE_CORNER_BOTTOM_LEFT, drawingColor);
            SetChar(x + width - 1, y, (char)PIXELS.LINE_CORNER_TOP_RIGHT, drawingColor);
            SetChar(x + width - 1, y + height - 1, (char)PIXELS.LINE_CORNER_BOTTOM_RIGHT, drawingColor);
        }

        public void SetChar(int x, int y, char c, short attributes = (short)COLOR.FG_WHITE)
        {
            var offset = _screenbuf.GetOffset(x, y);
            _screenbuf.Data[offset].Attributes = attributes;
            _screenbuf.Data[offset].Char.UnicodeChar = (short)c;
        }

        public char GetChar(int x, int y)
        {
            return (char)_screenbuf.GetData(x, y).Char.UnicodeChar;
        }

        public void Start()
        {

            _gamethread.Start();
            _gamethread.Join();
        }

        public static KeyState GetKeyState(ConsoleKey key)
        {
            return KeyStates[(int)key];
        }

        public static int Clamp(int v, int min, int max)
        {
            return Math.Min(Math.Max(v, min), max);
        }

        public static double ClampF(double v, double min, double max)
        {
            return Math.Min(Math.Max(v, min), max);
        }

        public abstract bool OnUserCreate();

        public abstract bool OnUserUpdate(TimeSpan elapsedTime);

        private void Paint()
        {
            WriteConsoleOutput(_consolehandle, _screenbuf.Data, _screencoord, _topleft, ref _screenrect);
        }

        private void GetKeyStates()
        {
            for (int i = 0; i < KEYSTATES; i++)
            {
                _newkeystate[i] = GetAsyncKeyState(i);

                KeyStates[i].Pressed = false;
                KeyStates[i].Released = false;

                if (_newkeystate[i] != _oldkeystate[i])
                {
                    if ((_newkeystate[i] & 0x8000) != 0)
                    {
                        KeyStates[i].Pressed = !KeyStates[i].Held;
                        KeyStates[i].Held = true;
                    }
                    else
                    {
                        KeyStates[i].Released = true;
                        KeyStates[i].Held = false;
                    }
                }
                _oldkeystate[i] = _newkeystate[i];
            }
        }

        public short ClosedConsoleColor3Bit(byte r, byte g, byte b, out char pixel)
        {
            short sixBitValue = (short)((r / 85) << 4 | (g / 85) << 2 | (b / 85));

            switch (sixBitValue)
            {
                case 0xFF: //white
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x000F;

                case 0x00:  //black
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0000;
                case 0x15: //dark grey
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0008;
                case 0x3F: //light grey
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0007;

                case 0x30: //red
                case 0x31:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x000C;
                case 0x20: //dark red
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0004;
                case 0x10: //darker red
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x000C;
                case 0x25: //blueish red
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x009C;

                case 0x03: //blue
                case 0x17:
                case 0x07:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0009;
                case 0x02: //dark blue
                case 0x06:
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x0001;
                case 0x01: //darker blue
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x0009;
                case 0x2F: //cornflower blue
                case 0x1B:
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x009F;
                case 0x05: //petrol
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x009A;
                case 0x0B: //light blue
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x00F9;

                case 0x0C: //green
                case 0x1C:
                case 0x1D:
                case 0x08:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x000A;
                case 0x18: //dark green
                case 0x19:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0002;
                case 0x04: //darker green
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x000A;
                case 0x14: //darkest green
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x000A;
                case 0x2C: //yellowish green
                case 0x2D:
                case 0x2E:
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x00EA;
                case 0x0A: //blueish green
                case 0x1A:
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x009A;

                case 0x0D: //leaf green
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x009A;
                case 0x0E: //frog green
                case 0x1E:
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x001A;
                case 0x0F: //cyan
                case 0x1F:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x000B;


                case 0x28: //dark yellow
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0006;
                case 0x3C: //yellow
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x00F6;
                case 0x3D: //light yellow
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x000E;
                case 0x3E: //lighter yellow
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x00FE;

                case 0x11: //dark purple
                case 0x26:
                    pixel = (char)PIXELS.PIXEL_HALF;
                    return 0x001C;
                case 0x12: //purple
                case 0x16:
                case 0x22:
                    pixel = (char)PIXELS.PIXEL_HALF;
                    return 0x009C;
                case 0x13: //light purple
                case 0x23:
                case 0x27:
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x009C;

                case 0x33: //magenta
                case 0x37:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x000D;
                case 0x32: //dark magenta
                case 0x36:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x0005;

                case 0x38: //orange
                case 0x34:
                case 0x35:
                    pixel = (char)PIXELS.PIXEL_HALF;
                    return 0x00EC;

                case 0x24: //brown
                    pixel = (char)PIXELS.PIXEL_THREEQUARTERS;
                    return 0x0096;
                case 0x29: //skintone'ish?
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x00EC;

                case 0x2A: //piggy colored
                    pixel = (char)PIXELS.PIXEL_QUARTER;
                    return 0x00FC;

                case 0x2B: //pink
                    pixel = (char)PIXELS.PIXEL_HALF;
                    return 0x00FC;

                case 0x21: //dark pink
                    pixel = (char)PIXELS.PIXEL_HALF;
                    return 0x00F4;

                default:
                    pixel = (char)PIXELS.PIXEL_SOLID;
                    return 0x000F;

            }
        }

        #region IDisposable Support
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects) here.
                }

                _consolehandle.Dispose();

                _disposed = true;
            }
        }

        ~GameConsole()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
