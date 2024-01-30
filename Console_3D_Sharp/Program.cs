//using Console_3D_Sharp;
using Console_3D_Sharp;
using ConsoleEngine;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Windows.Foundation;

class Program
{
    const int RAND_MAX = 2;

    static int nScreenWidth = 700;          // Console Screen Size X (columns)
    static int nScreenHeight = 160;          // Console Screen Size Y (rows)
    static int nMapWidth = 32;              // World Dimensions
    static int nMapHeight = 32;

    static double fPlayerX = 13.7;          // Player Start Position
    static double fPlayerY = 5.09;
    static double fPlayerA = -26.7;           // Player Start Rotation
    static double fFOV = 3.14159 / 4.0;    // Field of View
    static double fDepth = 32.0;            // Maximum rendering distance
    static double fSpeed = 5.0;             // Walking Speed

    static string map;

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

            #region define sprites
            int index = (int)COLOR.BG_DARK_GREY;
            string[] walltexture = new string[]
            {
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░",
            };
            COLOR[] wallColors = new COLOR[]
            {
             COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,
COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,
COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,
COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,
COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,
COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,COLOR.BG_DARK_GREY,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,
COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_GREY,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,COLOR.BG_DARK_RED,



            };
            wall = new Sprite(walltexture, wallColors);


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

            //mouseHandle = NativeMethods.GetStdHandle(NativeMethods.STD_INPUT_HANDLE);
            //int mode = 0;
            //if (!(NativeMethods.GetConsoleMode(mouseHandle, ref mode))) { throw new Win32Exception(); }
            //mode |= NativeMethods.ENABLE_MOUSE_INPUT;
            //mode &= ~NativeMethods.ENABLE_QUICK_EDIT_MODE;
            //mode |= NativeMethods.ENABLE_EXTENDED_FLAGS;
            //if (!(NativeMethods.SetConsoleMode(mouseHandle, mode))) { throw new Win32Exception(); }
            //mouseRecord = new NativeMethods.INPUT_RECORD();
            //mouseRecordLen = 0;

            return true;
        }

        public override bool OnUserUpdate(TimeSpan fElapsedTime)
        {      
            //for(byte i = 0; i <= 255; i++)
            //{
            //    SetChar(i, (int)(i/50), (char)i, (short)COLOR.FG_WHITE);
            //}

            //return true;


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
            #region Mouse --> Lag!
            //if (!(NativeMethods.ReadConsoleInput(mouseHandle, ref mouseRecord, 1, ref mouseRecordLen))) { throw new Win32Exception(); }

            //if(mouseRecord.EventType == NativeMethods.MOUSE_EVENT )
            //{
            //    mousePos2 = mouseRecord.MouseEvent.dwMousePosition.X;

            //    if ((mousePos1 - mousePos2) > 0)
            //        fPlayerA -= (fSpeed * 0.5f) * (fElapsedTime.TotalSeconds * 1.5);// (Math.Abs(mousePos1 - mousePos2) * 0.05);//fElapsedTime.TotalSeconds;
            //    else if ((mousePos1 - mousePos2) < 0)
            //        fPlayerA += (fSpeed * 0.5f) * (fElapsedTime.TotalSeconds * 1.5); //(Math.Abs(mousePos1 - mousePos2) * 0.05);//fElapsedTime.TotalSeconds;

            //    mousePos1 = mousePos2;

            //    System.Windows.Forms.Cursor.Hide();

            //    //Cursor.Position = mousePos1;

            //}
            #endregion
            #endregion

            Mode7(fPlayerY / 50, fPlayerX / 20, fPlayerA, 0.005, 0.03, 3.14159 / 4, ground, false);
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
                    else if(obj.sprite == fireball)
                    {
                        for (double lx = 0; lx < fObjectWidth; lx++)
                        {
                            for (double ly = 0; ly < fObjectHeight; ly++)
                            {
                                double fSampleX = lx / fObjectWidth *  2;
                                double fSampleY = ly / fObjectHeight * 2;
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
        // Debug(); return;
        //Console.OutputEncoding = Encoding.UTF8;
        Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

        //Console.WriteLine("What i want");
        //Console.WriteLine("░▒▓█");

        //Console.ReadKey();


        Encoding cp437 = Encoding.GetEncoding(437);
        byte[] source = new byte[1];
        AnsiLookup = new Dictionary<byte, string>();

        //Console.WriteLine("The shit i got");
        for (byte i = 0x20; i < 0xFE; i++)
        {
            source[0] = i;
            
            AnsiLookup.Add(i, cp437.GetString(source));

         //   Console.Write((char)i);
        }

        //Console.WriteLine("What i have to do....");
        //for(int i = 0x20; i < 0xFE; i++)
        //    Console.Write(AnsiLookup[i]);

        //Console.ReadKey();




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


    static void Debug()
    {
        Sprite wall = new Sprite("FPSSprites\\fps_wall1.spr");

        for(int x = 0; x < wall.Width; x++)
        {
            for(int y = 0; y < wall.Height; y++) 
            {
                Console.BackgroundColor = (ConsoleColor)wall.GetColor(x, y);
                Console.Write(wall.GetChar(x, y));
            }
            Console.WriteLine();
        }


    }
}




//public static class Keyboard
//{
//    [DllImport("user32.dll", SetLastError = true)]
//    [return: MarshalAs(UnmanagedType.Bool)]
//    static extern bool GetKeyboardState(byte[] lpKeyState);

//    public static int GetKeyState()
//    {


//        byte[] keys = new byte[256];

//        //Get pressed keys
//        if (!GetKeyboardState(keys))
//        {
//            int err = Marshal.GetLastWin32Error();
//            throw new Win32Exception(err);
//        }

//        for (int i = 0; i < 256; i++)
//        {

//            byte key = keys[i];

//            //Logical 'and' so we can drop the low-order bit for toggled keys, else that key will appear with the value 1!
//            if ((key & 0x80) != 0)
//            {

//                //This is just for a short demo, you may want this to return
//                //multiple keys!
//                return (int)key;
//            }
//        }
//        return -1;
//    }

//    [DllImport("user32.dll")]
//    static extern short GetKeyState(Key nVirtKey);

//    public static bool IsKeyPressed(Key testKey)
//    {
//        bool keyPressed = false;
//       short result = GetKeyState(testKey);

//        switch (result)
//        {
//            case 0:
//                // Not pressed and not toggled on.
//                keyPressed = false;
//                break;

//            case 1:
//                // Not pressed, but toggled on
//                keyPressed = false;
//                break;

//            default:
//                // Pressed (and may be toggled on)
//                keyPressed = true;
//                break;
//        }

//        return keyPressed;
//    }



//    private const uint MAPVK_VK_TO_CHAR = 2;

//    [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
//    static extern uint MapVirtualKeyW(uint uCode, uint uMapType);

//    public static char KeyToChar(Key key)
//    {
//        return unchecked((char)MapVirtualKeyW((uint)key, MAPVK_VK_TO_CHAR)); // Ignore high word.  
//    }
//}