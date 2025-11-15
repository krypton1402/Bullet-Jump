using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJumpLibrary.Graphics
{
    public class AnimatedSprite : Sprite
    {
        private int _currentFrame;
        private TimeSpan _elapsed;
        private Animation _animation;
        private bool _isPaused = false;

        /// <summary>
        /// Gets or Sets the animation for this animated sprite.
        /// </summary>
        public Animation Animation
        {
            get => _animation;
            set
            {
                _animation = value;
                ResetAnimation();
            }
        }

        /// <summary>
        /// Gets whether the animation is currently paused.
        /// </summary>
        public bool IsPaused => _isPaused;

        /// <summary>
        /// Gets the current frame index.
        /// </summary>
        public int CurrentFrameIndex => _currentFrame;

        public AnimatedSprite() { }

        public AnimatedSprite(Animation animation)
        {
            Animation = animation;
        }

        public void Update(GameTime gameTime)
        {
            if (_isPaused || _animation == null)
                return;

            _elapsed += gameTime.ElapsedGameTime;

            if (_elapsed >= _animation.Delay)
            {
                _elapsed -= _animation.Delay;
                _currentFrame++;

                if (_currentFrame >= _animation.Frames.Count)
                {
                    _currentFrame = 0;
                }

                // СОХРАНЯЕМ Origin перед сменой кадра!
                Vector2 oldOrigin = Origin;
                Region = _animation.Frames[_currentFrame];
                Origin = oldOrigin; // ВОССТАНАВЛИВАЕМ Origin
            }
        }

        public TextureRegion GetFrameByIndex(int index)
        {
            if (_animation == null || index < 0 || index >= _animation.Frames.Count)
                return null;
            return _animation.Frames[index];
        }

        public void SetFrame(int frameIndex)
        {
            if (_animation == null || frameIndex < 0 || frameIndex >= _animation.Frames.Count)
                return;

            // СОХРАНЯЕМ Origin перед сменой кадра!
            Vector2 oldOrigin = Origin;
            _currentFrame = frameIndex;
            Region = _animation.Frames[frameIndex];
            Origin = oldOrigin; // ВОССТАНАВЛИВАЕМ Origin
            _elapsed = TimeSpan.Zero;
        }

        public void ResetAnimation()
        {
            SetFrame(0);
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public void Resume()
        {
            _isPaused = false;
        }

        public void Stop()
        {
            Pause();
            ResetAnimation();
        }

        public void Play()
        {
            ResetAnimation();
            Resume();
        }

        public bool IsLastFrame()
        {
            return _animation != null && _currentFrame == _animation.Frames.Count - 1;
        }
    }
}