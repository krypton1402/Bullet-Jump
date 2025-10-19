using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJumpLibrary.Graphics.Interfaces
{
    public interface IAnimationController
    {
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch, Vector2 position);
        void SetEffects(SpriteEffects effects);
        void Play();
        void Stop();
        void Resume();
        void PlayImmediate();

        // Добавляем методы для получения размеров
        float Width { get; }
        float Height { get; }
        Vector2 Origin { get; }
        Vector2 Scale { get; }
    }
}
