using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleGameEngine;


class Program
{
    const int RAND_MAX = 2;

    static Dictionary<byte, string> AnsiLookup;

    class ConsoleFPS : GameConsole
    {
        public ConsoleFPS()
            : base(200, 120, "Shooter", fontwidth: 4, fontheight: 4)
        { }

        private int nMapWidth = 32;              // World Dimensions
        private int nMapHeight = 32;
        private double fPlayerX = 13.7;          // Player Start Position
        private double fPlayerY = 5.09;
        private double fPlayerA = -26.7;           // Player Start Rotation
        private double fFOV = 3.14159 / 4.0;    // Field of View
        private double fDepth = 16.0;            // Maximum rendering distance
        private double fSpeed = 5.0;             // Walking Speed
        private string map = "";
        private Sprite wall, lamp, fireball, ground;
        private double[] fDepthBuffer;
        private struct sObject
        {
            public double x;
            public double y;
            public double vx;
            public double vy;
            public bool bRemove;
            public Sprite sprite;
        };

        List<sObject> listObjects;
        animation coinAnim;

        public override bool OnUserCreate()
        {
            #region MAP
            map += "#########.......#########.......";
            map += "#...............#...............";
            map += "#.......#########.......########";
            map += "#..............##..............#";
            map += "#......##......##......##......#";
            map += "#......##..............##......#";
            map += "#..............##..............#";
            map += "###............####............#";
            map += "##.............###.............#";
            map += "#............####............###";
            map += "#..............................#";
            map += "#..............##..............#";
            map += "#..............##..............#";
            map += "#...........#####...........####";
            map += "#..............................#";
            map += "###..####....########....#######";
            map += "####.####.......######..........";
            map += "#...............#...............";
            map += "#.......#########.......##..####";
            map += "#..............##..............#";
            map += "#......##......##.......#......#";
            map += "#......##......##......##......#";
            map += "#..............##..............#";
            map += "###............####............#";
            map += "##.............###.............#";
            map += "#............####............###";
            map += "#..............................#";
            map += "#..............................#";
            map += "#..............##..............#";
            map += "#...........##..............####";
            map += "#..............##..............#";
            map += "################################";
            #endregion

            wall = new Sprite(@"wall2.txt");//("FPSSprites\\fps_wall1.spr");
            lamp = new Sprite(@"Imp.txt");
            fireball = new Sprite(@"FireBall.txt");
            ground = new Sprite("mk_track.txt");
            fDepthBuffer = new double[Width];

            coinAnim = new animation(new List<string> { @"Coin1.txt", @"Coin2.txt", @"Coin3.txt", @"Coin4.txt" }, new TimeSpan(0, 0, 0, 0, 500));

            listObjects = new List<sObject>
            {
                new sObject() { x = 8.5f, y = 8.5f, vx = 0.0f, vy = 0.0f, bRemove = false, sprite = lamp },
                new sObject() { x = 7.5f, y = 7.5f, vx = 0.0f, vy = 0.0f, bRemove = false, sprite = lamp },
                new sObject() { x = 10.5f, y = 3.5f, vx = 0.0f, vy = 0.0f, bRemove = false, sprite = lamp },
                new sObject() { x = 9.5f, y = 2.5f, vx = 0.0f, vy = 0.0f, bRemove = false, sprite = coinAnim.outputSprite },
            };

            return true;
        }

        public override bool OnUserUpdate(TimeSpan fElapsedTime)
        {      
            #region inputs
            if (GetKeyState(ConsoleKey.A).Held)// Keys[((int)'A')].held)
                fPlayerA -= (fSpeed * 0.5f) * fElapsedTime.TotalSeconds;

            if (GetKeyState(ConsoleKey.D).Held)//(Keys[((int)'D')].held)
                fPlayerA += (fSpeed * 0.5f) * fElapsedTime.TotalSeconds;

            if (GetKeyState(ConsoleKey.W).Held) //(Keys[((int)'W')].held)
            {
                fPlayerX += Math.Sin(fPlayerA) * fSpeed * (fElapsedTime.TotalMilliseconds / 1000); ;
                fPlayerY += Math.Cos(fPlayerA) * fSpeed * (fElapsedTime.TotalMilliseconds / 1000); ;

                if (map[(int)(fPlayerX * nMapWidth) + (int)fPlayerY] == '@')
                {
                    fPlayerX -= Math.Sin(fPlayerA) * fSpeed * (fElapsedTime.TotalMilliseconds / 1000); ;
                    fPlayerY -= Math.Cos(fPlayerA) * fSpeed * (fElapsedTime.TotalMilliseconds / 1000); ;
                }
            }

            if (GetKeyState(ConsoleKey.S).Held) //(Keys[((int)'S')].held)
            {
                fPlayerX -= Math.Sin(fPlayerA) * fSpeed * fElapsedTime.TotalSeconds; ;
                fPlayerY -= Math.Cos(fPlayerA) * fSpeed * fElapsedTime.TotalSeconds; ;
                if (map[(int)(fPlayerX * nMapWidth) + (int)fPlayerY] == '@')
                {
                    fPlayerX += Math.Sin(fPlayerA) * fSpeed * fElapsedTime.TotalSeconds; ;
                    fPlayerY += Math.Cos(fPlayerA) * fSpeed * fElapsedTime.TotalSeconds; ;
                }
            }

            if (GetKeyState(ConsoleKey.E).Held) //(Keys[((int)'E')].held)
            {
                fPlayerX += Math.Cos(fPlayerA) * fSpeed * fElapsedTime.TotalSeconds; ;
                fPlayerY -= Math.Sin(fPlayerA) * fSpeed * fElapsedTime.TotalSeconds; ;
                if (map[(int)(fPlayerX * nMapWidth) + (int)fPlayerY] == '@')
                {
                    fPlayerX -= Math.Cos(fPlayerA) * fSpeed * fElapsedTime.TotalSeconds; ;
                    fPlayerY += Math.Sin(fPlayerA) * fSpeed * fElapsedTime.TotalSeconds; ;
                }
            }

            if (GetKeyState(ConsoleKey.Q).Held) //(Keys[((int)'Q')].held)
            {
                fPlayerX -= Math.Cos(fPlayerA) * fSpeed * fElapsedTime.TotalSeconds; ;
                fPlayerY += Math.Sin(fPlayerA) * fSpeed * fElapsedTime.TotalSeconds; ;
                if (map[(int)(fPlayerX * nMapWidth) + (int)fPlayerY] == '@')
                {
                    fPlayerX += Math.Cos(fPlayerA) * fSpeed * fElapsedTime.TotalSeconds; ;
                    fPlayerY -= Math.Sin(fPlayerA) * fSpeed * fElapsedTime.TotalSeconds; ;
                }
            }

            if (GetKeyState(ConsoleKey.Spacebar).Released)//(Keys[32].released)
            {
                sObject o;
                o.x = fPlayerX;
                o.y = fPlayerY;
                float fNoise = (((float)new Random().Next(RAND_MAX) - 0.5f) * 0.1f);
                o.vx = Math.Sin(fPlayerA + fNoise) * 8.0f;
                o.vy = Math.Cos(fPlayerA + fNoise) * 8.0f;
                o.sprite = fireball;
                o.bRemove = false;
                listObjects.Add(o);
            }
            #endregion

            Mode7(fPlayerY / 128, fPlayerX / 128, fPlayerA, 0.005, 0.01, 3.14159 / 4, ground, false);
            #region Main-View
            for (int x = 0; x < Width; x++)
            {
                // For each column, calculate the projected ray angle into world space
                double fRayAngle = (fPlayerA - fFOV / 2.0f) + ((float)x / (float)Width) * fFOV;

                // Find distance to wall
                double fStepSize = 0.01f;      // Increment size for ray casting, decrease to increase	
                double fDistanceToWall = 0.0f; //                                      resolution

                bool bHitWall = false;      // Set when ray hits wall block
                bool bBoundary = false;     // Set when ray hits boundary between two wall blocks

                double fEyeX = Math.Sin(fRayAngle); // Unit vector for ray in player space
                double fEyeY = Math.Cos(fRayAngle);

                double fSampleX = 0.0f;

                bool bLit = false;

                while (!bHitWall && fDistanceToWall < fDepth)
                {
                    fDistanceToWall += fStepSize;
                    int nTestX = (int)(fPlayerX + fEyeX * fDistanceToWall);
                    int nTestY = (int)(fPlayerY + fEyeY * fDistanceToWall);

                    // Test if ray is out of bounds
                    if (nTestX < 0 || nTestX >= nMapWidth || nTestY < 0 || nTestY >= nMapHeight)
                    {
                        bHitWall = true;            // Just set distance to maximum depth
                        fDistanceToWall = fDepth;
                    }
                    else
                    {
                        // Ray is inbounds so test to see if the ray cell is a wall block
                        if (map[nTestX * nMapWidth + nTestY] == '#')
                        {
                            // Ray has hit wall
                            bHitWall = true;

                            // Determine where ray has hit wall. Break Block boundary
                            // int 4 line segments
                            double fBlockMidX = (double)nTestX + 0.5f;
                            double fBlockMidY = (double)nTestY + 0.5f;

                            double fTestPointX = fPlayerX + fEyeX * fDistanceToWall;
                            double fTestPointY = fPlayerY + fEyeY * fDistanceToWall;

                            double fTestAngle = Math.Atan2((fTestPointY - fBlockMidY), (fTestPointX - fBlockMidX));

                            if (fTestAngle >= -3.14159f * 0.25f && fTestAngle < 3.14159f * 0.25f)
                                fSampleX = fTestPointY - (double)nTestY;
                            if (fTestAngle >= 3.14159f * 0.25f && fTestAngle < 3.14159f * 0.75f)
                                fSampleX = fTestPointX - (double)nTestX;
                            if (fTestAngle < -3.14159f * 0.25f && fTestAngle >= -3.14159f * 0.75f)
                                fSampleX = fTestPointX - (double)nTestX;
                            if (fTestAngle >= 3.14159f * 0.75f || fTestAngle < -3.14159f * 0.75f)
                                fSampleX = fTestPointY - (double)nTestY;
                        }
                    }
                }

                // Calculate distance to ceiling and floor
                int nCeiling = (int)((double)(Height / 2.0) - (Height / fDistanceToWall)); //(int)((Height / 2.0 - Height) / (fDistanceToWall));
                int nFloor = Height - nCeiling;

                // Update Depth Buffer
                fDepthBuffer[x] = fDistanceToWall;

                for (int y = 0; y < Height; y++)
                {
                    // Each Row
                    if (y <= nCeiling)
                        SetChar(x, y, ' ', (short)GameConsole.COLOR.BG_BLACK);
                    else if (y > nCeiling && y <= nFloor)
                    {
                        // Draw Wall
                        if (fDistanceToWall < fDepth)
                        {

                            double fSampleY = ((double)y - (double)nCeiling) / ((double)nFloor - (double)nCeiling);
                            SetChar(x, y, (char)(GameConsole.PIXELS)(int)wall.SampleGlyph(fSampleX, fSampleY), wall.SampleColor(fSampleX, fSampleY));
                        }
                        else
                            SetChar(x, y, (char)GameConsole.PIXELS.PIXEL_SOLID, 0);
                    }
                    else // Floor
                    {
                        //Commented for mode7
                        //SetChar(x, y, (char)GameConsole.PIXELS.PIXEL_HALF,  (short)GameConsole.COLOR.FG_DARK_GREEN); //,(char)GameConsole.PIXELS.PIXEL_SOLID //encs.GetString(new byte[1] { 176 })[0]
                    }
                }
            }
            #endregion

            #region Game-Objects

            coinAnim.Update();
            var coin = listObjects.ElementAt(3);
            coin.sprite = coinAnim.outputSprite;
            listObjects[3] = coin;

            // Update & Draw Objects	
            for (int i = 0; i < listObjects.Count; i++)
            {
                var obj = listObjects[i];
                // Update Object Physics
                obj.x += obj.vx * fElapsedTime.TotalSeconds;
                obj.y += obj.vy * fElapsedTime.TotalSeconds;

                // Check if object is inside wall - set flag for removal
                if (map[(int)obj.x * nMapWidth + (int)obj.y] == '#')
                    obj.bRemove = true;

                // Can object be seen?
                double fVecX = obj.x - fPlayerX;
                double fVecY = obj.y - fPlayerY;
                double fDistanceFromPlayer = Math.Sqrt(fVecX * fVecX + fVecY * fVecY);

                double fEyeX = Math.Sin(fPlayerA);
                double fEyeY = Math.Cos(fPlayerA);

                // Calculate angle between lamp and players feet, and players looking direction
                // to determine if the lamp is in the players field of view
                double fObjectAngle = Math.Atan2(fEyeY, fEyeX) - Math.Atan2(fVecY, fVecX);
                if (fObjectAngle < -3.14159f)
                    fObjectAngle += 2.0f * 3.14159f;
                if (fObjectAngle > 3.14159f)
                    fObjectAngle -= 2.0f * 3.14159f;

                bool bInPlayerFOV = Math.Abs(fObjectAngle) < fFOV / 2.0f;

                if (bInPlayerFOV && fDistanceFromPlayer >= 0.5f && fDistanceFromPlayer < fDepth && !obj.bRemove)
                {
                    double fObjectCeiling = (Height / 2.0) - (double)Height / ((float)fDistanceFromPlayer);
                    double fObjectFloor = Height - fObjectCeiling;
                    double fObjectHeight = fObjectFloor - fObjectCeiling;
                    double fObjectAspectRatio = (double)obj.sprite.Height / (double)obj.sprite.Width;
                    double fObjectWidth = fObjectHeight / fObjectAspectRatio;
                    double fMiddleOfObject = (0.5f * (fObjectAngle / (fFOV / 2.0f)) + 0.5f) * Width;

                    if (obj.sprite == lamp || true)
                    {
                        // Draw Lamp
                        for (double lx = 0; lx < fObjectWidth; lx++)
                        {
                            for (double ly = 0; ly < fObjectHeight; ly++)
                            {
                                double fSampleX = lx / fObjectWidth;
                                double fSampleY = ly / fObjectHeight;
                                char c = obj.sprite.SampleGlyph(fSampleX, fSampleY);
                                int nObjectColumn = (int)(fMiddleOfObject + lx - (fObjectWidth / 2.0f));
                                if (nObjectColumn >= 0 && nObjectColumn < Width)
                                {
                                    if (c != ' ' && fDepthBuffer[nObjectColumn] >= fDistanceFromPlayer)
                                    {
                                        SetChar(nObjectColumn, (int)(fObjectCeiling + ly), c, obj.sprite.SampleColor(fSampleX, fSampleY));
                                        fDepthBuffer[nObjectColumn] = fDistanceFromPlayer;
                                    }
                                }
                            }
                        }
                    }
                }
            
                listObjects[i] = obj;
            }

            // Remove dead objects from object list
            listObjects.RemoveAll(s => s.bRemove);
            #endregion

            #region GUI
            // Display Map & Player
            for (int nx = 0; nx < nMapWidth; nx++)
                for (int ny = 0; ny < nMapWidth; ny++)
                    SetChar(nx + 1, ny + 1, map[ny * nMapWidth + nx]);
            SetChar(1 + (int)fPlayerY, 1 + (int)fPlayerX, 'P', (short)COLOR.BG_RED);
            #endregion

            return true;
        }
    }


    [STAThread]
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

        using (var f = new ConsoleFPS())
            f.Start();
    }
    
    static float GetElapsedTime()
    {
        TimeSpan elapsed = DateTime.Now - lastTime;
        lastTime = DateTime.Now;
        return (float)elapsed.TotalSeconds;
    }

    static DateTime lastTime = DateTime.Now;
}



