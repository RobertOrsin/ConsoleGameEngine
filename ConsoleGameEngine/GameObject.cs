using System;
using System.Collections.Generic;

namespace ConsoleGameEngine
{
    public class GameObject
    {
        readonly List<Animation> _animations = new List<Animation>();
        int ActiveAnimationIndex {get; set;}
        public Sprite outputSprite = null;


        public GameObject() 
        {
            outputSprite = new Sprite(16,16);
            AddAnimation(new Animation(outputSprite, new TimeSpan(0, 0, 0, 0, 100), 16, 16, 0));
            ActiveAnimationIndex = 0;
        }
        public GameObject(Sprite spriteSheet, int w, int h, TimeSpan timeSpan, List<int> frameCounts)
        {
            _animations = new List<Animation>();
            _animations = LoadSpriteSheet(spriteSheet, w, h, timeSpan, frameCounts);
        }

        public void Update()
        {
            _animations[ActiveAnimationIndex].Update();
            outputSprite = _animations[ActiveAnimationIndex].outputSprite;
        }

        public void AddAnimation(Animation animation)
        {
            _animations.Add(animation);
        }

        private List<Animation> LoadSpriteSheet(Sprite spriteSheet, int w, int h, TimeSpan timeSpan, List<int> frameCounts)
        {
            var animations = new List<Animation>();

            var width = spriteSheet.Width;
            var height = spriteSheet.Height;

            for(var i = 0; i < height / h; i++)
            {
                var newSprite = new Sprite(width, h);

                for(var x = 0; x < width; x++)
                {
                    for(var y = i * h; y < i * h + h; y++)
                    {
                        newSprite.SetPixel(x, y - i * h, spriteSheet.GetChar(x,y), spriteSheet.GetColor(x,y));
                    }
                }

                animations.Add(new Animation(newSprite, timeSpan, w, h, frameCounts[i]));
            }
            return animations;
        }

        public void DecAnimationIndex()
        {
            ActiveAnimationIndex--;

            if (ActiveAnimationIndex < 0)
                ActiveAnimationIndex = _animations.Count - 1;
        }
        public void IncAnimationIndex()
        {
            ActiveAnimationIndex++;

            if(ActiveAnimationIndex > _animations.Count - 1) 
                ActiveAnimationIndex = 0;
        }
    }
}
