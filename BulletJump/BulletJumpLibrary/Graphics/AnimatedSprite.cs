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

                Region = _animation.Frames[_currentFrame];
            }
        }

        //Получает кадр из анимации
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

                _currentFrame = frameIndex;
                Region = _animation.Frames[frameIndex];
                _elapsed = TimeSpan.Zero; // Сбрасываем таймер
        }

        public void ResetAnimation()
        {
            SetFrame(0);
        }

        /// <summary>
        /// Приостанавливает анимацию
        /// </summary>
        public void Pause()
        {
            _isPaused = true;
        }

        /// <summary>
        /// Возобновляет анимацию
        /// </summary>
        public void Resume()
        {
            _isPaused = false;
        }

        /// <summary>
        /// Останавливает анимацию и сбрасывает на первый кадр
        /// </summary>
        public void Stop()
        {
            Pause();
            ResetAnimation();
        }

        /// <summary>
        /// Запускает анимацию с первого кадра
        /// </summary>
        public void Play()
        {
            ResetAnimation();
            Resume();
        }

        /// <summary>
        /// Проверяет, является ли текущий кадр последним в анимации
        /// </summary>
        public bool IsLastFrame()
        {
            return _animation != null && _currentFrame == _animation.Frames.Count - 1;
        }
    }
}
