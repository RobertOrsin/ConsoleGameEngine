using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

using OpenCvSharp;
using BigGustave;

using ConsoleGameEngine;
using static ConsoleGameEngine._3DEngine;
using static ConsoleGameEngine.GameConsole;
using static ConsoleGameEngine.NativeMethods;
using System.Reflection.Emit;
using System.IO;

namespace YouTubeStream
{
    class YouTubeStream : GameConsole
    {
        string streamURL = @"https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        VideoCapture videoCapture;

        public YouTubeStream()
          : base(200, 120, "Fonts", fontwidth: 4, fontheight: 4)
        { }
        public override bool OnUserCreate()
        {
            var youtube = new YoutubeClient();
            var videoId = "OWBFKL6H7rI";

            // Get stream URL synchronously
            var streamManifest = youtube.Videos.Streams.GetManifestAsync(videoId).GetAwaiter().GetResult();

            var streamInfo = streamManifest.GetMuxedStreams().TryGetWithHighestVideoQuality();
            var videoStream = streamManifest.GetVideoOnlyStreams().OrderByDescending(s => s.VideoQuality.MaxHeight).LastOrDefault();


            if (videoStream == null)
                throw new Exception("No video-only streams found.");

            string streamUrl = videoStream.Url;

            videoCapture = new VideoCapture(streamUrl);

            if (!videoCapture.IsOpened()) return false;


            return true;
        }
        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            Mat frame = new Mat();
            videoCapture.Read(frame);

            byte[] pngBytes = frame.ToBytes(".png");
            var ms = new MemoryStream(pngBytes);
            Png png = Png.Open(ms);

            Sprite outputsprite = new Sprite(png.Width, png.Height);
            for (int x = 0; x < png.Width; x++)
            {
                for (int y = 0; y < png.Height; y++)
                {
                    byte red = png.GetPixel(x, y).R;
                    byte green = png.GetPixel(x, y).G;
                    byte blue = png.GetPixel(x, y).B;

                    short col = ClosedConsoleColor3Bit(red, green, blue, out char pixel);

                    outputsprite.SetPixel(x, y, pixel, col);
                }
            }
            DrawSprite(0, 0, outputsprite);

            return true;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);

            using (var f = new YouTubeStream())
                f.Start();
        }
    }
}