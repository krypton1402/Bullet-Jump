using BulletJumpLibrary.Graphics.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJumpLibrary.Graphics.Animations
{
    public class AnimationController : IAnimationController
    {
        private AnimatedSprite _sprite;

        public AnimationController(AnimatedSprite sprite)
        {
            _sprite = sprite;
        }

        public void Update(GameTime gameTime) => _sprite.Update(gameTime);
        public void Draw(SpriteBatch spriteBatch, Vector2 position) => _sprite.Draw(spriteBatch, position);
        public void SetEffects(SpriteEffects effects) => _sprite.Effects = effects;
        public void Play() => _sprite.Play();
        public void Stop() => _sprite.Stop();
        public void Resume() => _sprite.Resume();
        public void SetFrame(int frameIndex) => _sprite.SetFrame(frameIndex);

        // Реализуем свойства для размеров
        public float Width => _sprite.Width;
        public float Height => _sprite.Height;
        public Vector2 Origin => _sprite.Origin;
        public Vector2 Scale => _sprite.Scale;

        public bool IsPlaying => !_sprite.IsPaused;

        public void PlayImmediate()
        {
            _sprite.Play(); // Сбрасываем на первый кадр
            _sprite.SetFrame(1); // Немедленно переходим ко второму кадру
            _sprite.Resume(); // Запускаем воспроизведение
        }
    }
}
