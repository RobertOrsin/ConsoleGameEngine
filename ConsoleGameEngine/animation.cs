using System;
using System.Collections.Generic;


namespace ConsoleGameEngine
{
    public class Animation
    {
        readonly List<Sprite> _sprites;
        public Sprite outputSprite;
        readonly TimeSpan _frameDelay;
        DateTime _lastUpdate;
        int _shownFrame;
        readonly int _frameWidth, _frameHeight;
        private readonly bool _animationFromOneFrame = false;
        private readonly int _frameCount = 0;

        public Animation(List<Sprite> sprites, TimeSpan frameDelay)
        {
            this._sprites = sprites;
            this._frameDelay = frameDelay;
            this._shownFrame = 0;
            _lastUpdate = DateTime.Now;
        }
        public Animation(Sprite sprite, TimeSpan frameDelay, int frameWidth, int frameHeight, int frameCount)
        {
            _animationFromOneFrame = true;
            _sprites = new List<Sprite> { sprite };
            this._frameDelay = frameDelay;
            this._shownFrame = 0;
            _lastUpdate = DateTime.Now;
            this._frameWidth = frameWidth;
            this._frameHeight = frameHeight;
            outputSprite = _sprites[0].ReturnPartialSprite(_shownFrame * frameWidth, 0, frameWidth, frameHeight);
            this._frameCount = frameCount;
        }

        public Animation(List<string> sprites, TimeSpan frameDelay)
        {
            this._sprites = new List<Sprite>();
            foreach (string sprite in sprites)
            {
                this._sprites.Add(new Sprite(sprite));
            }
            outputSprite = this._sprites[this._shownFrame];
            this._frameDelay = frameDelay;
            this._shownFrame = 0;
            _lastUpdate = DateTime.Now;
        }

        public Animation(string sprite, TimeSpan frameDelay, int frameWidth, int frameHeight)
        {
            _animationFromOneFrame = true;
            _sprites = new List<Sprite> { new Sprite(sprite) };
            this._frameDelay = frameDelay;
            this._shownFrame = 0;
            _lastUpdate = DateTime.Now;
            this._frameWidth = frameWidth;
            this._frameHeight = frameHeight;
            outputSprite = _sprites[0].ReturnPartialSprite(_shownFrame * frameWidth, frameHeight, frameWidth, frameHeight);
            this._frameCount = _sprites[0].Width / frameWidth;
        }

        public void Update()
        {
            if(_frameDelay < DateTime.Now - _lastUpdate)
            {
                _lastUpdate = DateTime.Now;
                _shownFrame++;

                if (!_animationFromOneFrame)
                {
                    if (_shownFrame >= _sprites.Count)
                        _shownFrame = 0;
                    outputSprite = _sprites[_shownFrame];
                }
                else
                {
                    if (_shownFrame >= _sprites[0].Width / _frameWidth || _shownFrame >= _frameCount)
                        _shownFrame = 0;
                    outputSprite = _sprites[0].ReturnPartialSprite(_shownFrame * _frameWidth, 0, _frameWidth, _frameHeight);
                }
            }
        }
    }
}
