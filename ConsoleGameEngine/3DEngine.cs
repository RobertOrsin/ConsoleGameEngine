using BigGustave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ConsoleGameEngine
{
    public class _3DEngine
    {
        private double[] cos = new double[360];
        private double[] sin = new double[360];

        private int screenHeight, screenWidth;

        TimeSpan buttonDelay = new TimeSpan();
        TimeSpan buttonTime = new TimeSpan(0, 0, 0, 0, 60);

        public Sprite screen;

        public struct Player
        {
            public int x, y, z;    //player Position
            public int a;          //Player rotation (left and right)
            public int l;          //look up and down
        }
        public Player player;

        public struct Wall
        {
            public int bottomLinePoint1X, bottomLinePoint1Y;
            public int bottomLinePoint2X, bottomLinePoint2Y;
            public short color;
        }
        public Wall[] walls = new Wall[16];

        public struct Sector
        {
            public int wallNumberStart, wallNumberEnd;
            public int bottomHeight, topHeight;
            public int distance;
            public short colorBottom, colorTop;
            public  int[] surfacePoints;
            public int surface;
        }
        public Sector[] sectors = new Sector[4];



        #region testing
        int[] loadSectors = new int[]
        {//wall start, wall end, z1 height, z2 height, bottom color, top color
         0,  4, 0, 60, (int)GameConsole.COLOR.FG_BLUE ,(int)GameConsole.COLOR.FG_DARK_BLUE, //sector 1
         4,  8, 0, 40, (int)GameConsole.COLOR.FG_RED ,(int)GameConsole.COLOR.FG_DARK_RED, //sector 2
         8, 12, 0, 40, (int)GameConsole.COLOR.FG_GREEN ,(int)GameConsole.COLOR.FG_DARK_GREEN, //sector 3
         12,16, 0, 40, (int)GameConsole.COLOR.FG_YELLOW ,(int)GameConsole.COLOR.FG_DARK_YELLOW, //sector 4
        };

        int[] loadWalls = new int[]
        {//x1,y1, x2,y2, color
          0, 0, 32, 0, (int)GameConsole.COLOR.FG_RED,
         32, 0, 32,32, (int)GameConsole.COLOR.FG_DARK_RED,
         32,32,  0,32, (int)GameConsole.COLOR.FG_RED,
          0,32,  0, 0, (int)GameConsole.COLOR.FG_DARK_RED,

         64, 0, 96, 0, (int)GameConsole.COLOR.FG_GREEN,
         96, 0, 96,32, (int)GameConsole.COLOR.FG_DARK_GREEN,
         96,32, 64,32, (int)GameConsole.COLOR.FG_GREEN,
         64,32, 64, 0, (int)GameConsole.COLOR.FG_DARK_GREEN,

         64, 64, 96, 64, (int)GameConsole.COLOR.FG_YELLOW,
         96, 64, 96, 96, (int)GameConsole.COLOR.FG_DARK_YELLOW,
         96, 96, 64, 96, (int)GameConsole.COLOR.FG_YELLOW,
         64, 96, 64, 64, (int)GameConsole.COLOR.FG_DARK_YELLOW,

          0, 64, 32, 64, (int)GameConsole.COLOR.FG_BLUE,
         32, 64, 32, 96, (int)GameConsole.COLOR.FG_DARK_BLUE,
         32, 96,  0, 96, (int)GameConsole.COLOR.FG_BLUE,
          0, 96,  0, 64, (int)GameConsole.COLOR.FG_DARK_BLUE,
        };

        #endregion

        public _3DEngine(int _screenHeight, int _screenWidth)
        {
            screenHeight = _screenHeight;
            screenWidth = _screenWidth;

            screen = new Sprite(screenWidth, screenHeight);
            //sin/cos lookup-table
            for(int x  = 0; x < 360; x++)
            {
                cos[x] = Math.Cos(x / 180.0 * Math.PI);
                sin[x] = Math.Sin(x / 180.0 * Math.PI);
            }
            //init player
            player = new Player();
            player.x = 70; player.y = -110; player.z = 10; player.a = 0; player.l = 0;

            //load sectors
            int v1 = 0, v2 = 0;
            for(int s = 0; s < sectors.Count(); s++)
            {
                sectors[s].wallNumberStart = loadSectors[v1 + 0];
                sectors[s].wallNumberEnd = loadSectors[v1 + 1];
                sectors[s].bottomHeight = loadSectors[v1 + 2];
                sectors[s].topHeight = loadSectors[v1 + 3] - loadSectors[v1 + 2];
                sectors[s].colorTop = (short)loadSectors[v1 + 4];
                sectors[s].colorBottom = (short)loadSectors[v1 + 5];
                sectors[s].surfacePoints = new int[200];
                v1 += 6;

                for (int w = sectors[s].wallNumberStart; w < sectors[s].wallNumberEnd; w++)
                {
                    walls[w].bottomLinePoint1X = loadWalls[v2 + 0];
                    walls[w].bottomLinePoint1Y = loadWalls[v2 + 1];
                    walls[w].bottomLinePoint2X = loadWalls[v2 + 2];
                    walls[w].bottomLinePoint2Y = loadWalls[v2 + 3];
                    walls[w].color = (short)loadWalls[v2 + 4];
                    v2 += 5;
                }
            }
        }
        public void MovePlayer(TimeSpan elapsedTime)
        {
            buttonDelay += elapsedTime;
            if (buttonDelay >= buttonTime)
            {
                //Move up, down, left, right
                if (GameConsole.GetKeyState(ConsoleKey.A).Held && !GameConsole.GetKeyState(ConsoleKey.M).Held)
                {
                    player.a -= 4;
                    if (player.a < 0)
                        player.a += 360;
                }
                if (GameConsole.GetKeyState(ConsoleKey.D).Held && !GameConsole.GetKeyState(ConsoleKey.M).Held)
                {
                    player.a += 4;
                    if (player.a > 359)
                        player.a -= 360;
                }
                int dx = Convert.ToInt32(sin[player.a] * 10.0);
                int dy = Convert.ToInt32(cos[player.a] * 10.0);
                if (GameConsole.GetKeyState(ConsoleKey.W).Held && !GameConsole.GetKeyState(ConsoleKey.M).Held)
                {
                    player.x += dx;
                    player.y += dy;
                }
                if (GameConsole.GetKeyState(ConsoleKey.S).Held && !GameConsole.GetKeyState(ConsoleKey.M).Held)
                {
                    player.x -= dx;
                    player.y -= dy;
                }

                //strave left and right
                if (GameConsole.GetKeyState(ConsoleKey.Q).Held)
                {
                    player.x -= dy;
                    player.y += dx;
                }
                if (GameConsole.GetKeyState(ConsoleKey.E).Held)
                {
                    player.x += dy;
                    player.y -= dx;
                }

                //move up/down; look up/down
                if (GameConsole.GetKeyState(ConsoleKey.A).Held && GameConsole.GetKeyState(ConsoleKey.M).Held)
                    player.l -= 1;
                if (GameConsole.GetKeyState(ConsoleKey.D).Held && GameConsole.GetKeyState(ConsoleKey.M).Held)
                    player.l += 1;
                if (GameConsole.GetKeyState(ConsoleKey.W).Held && GameConsole.GetKeyState(ConsoleKey.M).Held)
                    player.z -= 4;
                if (GameConsole.GetKeyState(ConsoleKey.S).Held && GameConsole.GetKeyState(ConsoleKey.M).Held)
                    player.z += 4;

                buttonDelay = new TimeSpan();
            }
        }
        private void ClipBehindPlayer(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            double da = y1; // distance plane -> point a
            double db = y2; // distance plane -> point b
            double d = da - db;
            if (d == 0) { d = 1; }
            double s = da / (da - db); // intersection factor (between 0 and 1)
            x1 = (int)(x1 + s * (x2 - x1));
            y1 = (int)(y1 + s * (y2 - y1));
            if (y1 == 0) { y1 = 1; } // prevent divide by zero
            z1 = (int)(z1 + s * (z2 - z1));
        }
        private int Distance(int x1, int y1, int x2, int y2)
        {
            int distance = (int)Math.Sqrt(((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)));
            return distance;
        }
        public void Draw3D()
        {
            //Clear screen
            screen = new Sprite(screenWidth, screenHeight);

            //sort sectors
            for (int s = 0; s < sectors.Count() - 1; s++)
            {
                for (int w = 0; w < sectors.Count() - s - 1; w++)
                {
                    if (sectors[w].distance < sectors[s + 1].distance)
                    {
                        Sector sector = sectors[w];
                        sectors[w] = sectors[w + 1];
                        sectors[w + 1] = sector;
                    }
                }
            }

            double SW2 = screenWidth / 2, SH2 = screenHeight / 2;
            int[] wx = new int[4];
            int[] wy = new int[4];
            int[] wz = new int[4];
            double CS = cos[player.a], SN = sin[player.a];
            int cycles;

            //draw sectors
            for (int s = 0; s < sectors.Count(); s++)
            {
                sectors[s].distance = 0; //clear distance

                if (player.z < sectors[s].bottomHeight) //bottom surface
                { 
                    sectors[s].surface = 1; cycles = 2;
                    for (int x = 0; x < screenWidth; x++)
                        sectors[s].surfacePoints[x] = screenHeight;
                } 
                else if (player.z > sectors[s].topHeight) //top surface
                { 
                    sectors[s].surface = 2; cycles = 2;
                    for (int x = 0; x < screenWidth; x++)
                        sectors[s].surfacePoints[x] = 0;
                } 
                else { sectors[s].surface = 0; cycles = 1; }    //no surfaces

                for (int frontBack = 0; frontBack < cycles; frontBack++)
                {
                    for (int w = sectors[s].wallNumberStart; w < sectors[s].wallNumberEnd; w++)
                    {
                        //offset bottom 2 points by player
                        int x1 = walls[w].bottomLinePoint1X - player.x, y1 = walls[w].bottomLinePoint1Y - player.y;
                        int x2 = walls[w].bottomLinePoint2X - player.x, y2 = walls[w].bottomLinePoint2Y - player.y;
                        //swap for surface
                        if (frontBack == 1)
                        {
                            int swap = x1;
                            x1 = x2;
                            x2 = swap;
                            swap = y1;
                            y1 = y2;
                            y2 = swap;
                        }

                        //world x position
                        wx[0] = Convert.ToInt32((x1 * CS) - (y1 * SN));
                        wx[1] = Convert.ToInt32((x2 * CS) - (y2 * SN));
                        wx[2] = wx[0];
                        wx[3] = wx[1];
                        //world y position
                        wy[0] = Convert.ToInt32((y1 * CS) + (x1 * SN));
                        wy[1] = Convert.ToInt32((y2 * CS) + (x2 * SN));
                        wy[2] = wy[0];
                        wy[3] = wy[1];
                        sectors[s].distance += Distance(0, 0, (wx[0] + wx[1]) / 2, (wy[0] + wy[1]) / 2);
                        //world z position
                        wz[0] = 0 - player.z + Convert.ToInt32(((player.l * wy[0] / 32.0)));
                        wz[1] = 0 - player.z + Convert.ToInt32(((player.l * wy[1] / 32.0)));
                        wz[2] = wz[0] + sectors[s].topHeight;
                        wz[3] = wz[1] + sectors[s].topHeight;

                        //check if wall is behind player
                        if (wy[0] < 1 && wy[1] < 1) continue;

                        if (wy[0] < 1)
                        {
                            ClipBehindPlayer(wx[0], wy[0], wz[0], wx[1], wy[1], wz[1]);
                            ClipBehindPlayer(wx[2], wy[2], wz[2], wx[3], wy[3], wz[3]);
                        }
                        if (wy[1] < 1)
                        {
                            ClipBehindPlayer(wx[1], wy[1], wz[1], wx[0], wy[0], wz[0]);
                            ClipBehindPlayer(wx[3], wy[3], wz[3], wx[2], wy[2], wz[2]);
                        }

                        //screen x and y position
                        wx[0] = Convert.ToInt32(((wx[0] * 200.0) / wy[0]) + SW2);// + screenWidth; 
                        wy[0] = Convert.ToInt32(((wz[0] * 200.0) / wy[0]) + SH2);
                        wx[1] = Convert.ToInt32(((wx[1] * 200.0) / wy[1]) + SW2);// + screenWidth; 
                        wy[1] = Convert.ToInt32(((wz[1] * 200.0) / wy[1]) + SH2);
                        wx[2] = Convert.ToInt32(((wx[2] * 200.0) / wy[2]) + SW2);// + screenWidth;
                        wy[2] = Convert.ToInt32(((wz[2] * 200.0) / wy[2]) + SH2);
                        wx[3] = Convert.ToInt32(((wx[3] * 200.0) / wy[3]) + SW2);// + screenWidth;
                        wy[3] = Convert.ToInt32(((wz[3] * 200.0) / wy[3]) + SH2);

                        DrawWall(wx[0], wx[1], wy[0], wy[1], wy[2], wy[3], walls[w].color, s);
                    }
                    sectors[s].distance /= (sectors[s].wallNumberEnd - sectors[s].wallNumberStart);
                }
            }   
        }
        public void DrawWall(int x1, int x2, int b1, int b2, int t1, int t2, short color, int s)
        {
            int deltaBottomLine = b2 - b1;
            int deltaTopLine = t2 - t1;
            int deltaX = x2 - x1;

            if (deltaX == 0)
                deltaX = 1;

            int xStart = x1;

            //CLIP X
            if (x1 < 1) x1 = 1;
            if (x2 < 1) x2 = 1;
            if(x1 >screenWidth - 1) x1 = screenWidth - 1;
            if(x2 >screenWidth - 1)x2 = screenWidth - 1;
            //draw x vertile lines
            for(int x = x1; x < x2; x++)
            {
                //Y start and end point
                int y1 = deltaBottomLine * (x - xStart) / deltaX+b1; //y bottom point
                int y2 = deltaTopLine * (x - xStart) / deltaX + t1; //y top point

                //CLIP Y
                if (y1 < 1) y1 = 1;
                if (y2 < 1) y2 = 1;
                if (y1 > screenHeight - 1) y1 = screenHeight - 1;
                if (y2 > screenHeight - 1) y2 = screenHeight - 1;

                //surface
                if (sectors[s].surface == 1) { sectors[s].surfacePoints[x] = y1; continue; }
                if (sectors[s].surface == 2) { sectors[s].surfacePoints[x] = y2; continue; }
                if (sectors[s].surface == -1)
                    for(int y = sectors[s].surfacePoints[x]; y < y1; y++)
                        screen.SetPixel(x, y, (char)GameConsole.PIXELS.PIXEL_SOLID, sectors[s].colorBottom);
                if (sectors[s].surface == -2)
                    for (int y = y2; y < sectors[s].surfacePoints[x]; y++)
                        screen.SetPixel(x, y, (char)GameConsole.PIXELS.PIXEL_SOLID, sectors[s].colorTop);

                for (int y = y1; y < y2; y++)
                    screen.SetPixel(x, y, (char)GameConsole.PIXELS.PIXEL_SOLID, color);
            }
        }
    }
}
