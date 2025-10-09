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

        /// <summary>
        /// Gets or Sets the animation for this animated sprite.
        /// </summary>
        public Animation Animation
        {
            get => _animation;
            set
            {
                _animation = value;
                Region = _animation.Frames[0];
            }
        }

        /// <summary>
        /// Creates a new animated sprite.
        /// </summary>
        public AnimatedSprite() { }

        /// <summary>
        /// Создает новый анимированный спрайт с заданными кадрами и задержкой.
        /// </summary>
        /// <param name="animation">The animation for this animated sprite.</param>
        public AnimatedSprite(Animation animation)
        {
            Animation = animation;
        }

        /// <summary>
        /// Updates this animated sprite.
        /// </summary>
        /// <param name="gameTime">A snapshot of the game timing values provided by the framework.</param>
        public void Update(GameTime gameTime)
        {
            _elapsed += gameTime.ElapsedGameTime;

            if (_elapsed >= _animation.Delay)
            {
                _elapsed -= _animation.Delay;
                _currentFrame++;

                if (_currentFrame >= _animation.Frames.Count)
                {
                    _currentFrame = 0;
                }

                Region = _animation.Frames[_currentFrame];
            }
        }
    }
}
