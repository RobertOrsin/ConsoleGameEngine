using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGameEngine
{
    public class animation
    {
        List<Sprite> sprites;
        public Sprite outputSprite;
        TimeSpan frameDelay;
        DateTime lastUpdate;
        int shownFrame;
        int frameWidth, frameHeight;
        private bool animationFromOneFrame = false;

        public animation(List<Sprite> sprites, TimeSpan frameDelay)
        {
            this.sprites = sprites;
            this.frameDelay = frameDelay;
            this.shownFrame = 0;
            lastUpdate = DateTime.Now;
        }
        public animation(Sprite sprite, TimeSpan frameDelay, int frameWidth, int frameHeight)
        {
            animationFromOneFrame = true;
            sprites = new List<Sprite> { sprite };
            this.frameDelay = frameDelay;
            this.shownFrame = 0;
            lastUpdate = DateTime.Now;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            outputSprite = sprites[0].ReturnPartialSprite(shownFrame * frameWidth, frameHeight, frameWidth, frameHeight);
        }

        public animation(List<string> sprites, TimeSpan frameDelay)
        {
            this.sprites = new List<Sprite>();
            foreach (string sprite in sprites)
            {
                this.sprites.Add(new Sprite(sprite));
            }
            outputSprite = this.sprites[this.shownFrame];
            this.frameDelay = frameDelay;
            this.shownFrame = 0;
            lastUpdate = DateTime.Now;
        }

        public void Update()
        {
            if(frameDelay < DateTime.Now - lastUpdate)
            {
                lastUpdate = DateTime.Now;
                shownFrame++;



                if (!animationFromOneFrame)
                {
                    if (shownFrame >= sprites.Count)
                        shownFrame = 0;
                    outputSprite = sprites[shownFrame];
                }
                else
                {
                    if (shownFrame >= sprites[0].Width / frameWidth)
                        shownFrame = 0;
                    outputSprite = sprites[0].ReturnPartialSprite(shownFrame * frameWidth, 0, frameWidth, frameHeight);
                }
            }
        }
    }
}
