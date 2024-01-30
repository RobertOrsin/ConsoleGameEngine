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

        public animation(List<Sprite> sprites, TimeSpan frameDelay)
        {
            this.sprites = sprites;
            this.frameDelay = frameDelay;
            this.shownFrame = 0;
            lastUpdate = DateTime.Now;
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

                if(shownFrame >= sprites.Count)
                    shownFrame = 0;

                outputSprite = sprites[shownFrame];
            }
        }




    }
}
