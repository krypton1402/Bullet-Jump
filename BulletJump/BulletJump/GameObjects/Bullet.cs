using BulletJump.Enums;
using BulletJumpLibrary;
using BulletJumpLibrary.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BulletJump.GameObjects
{
    public class Bullet
    {
        public Vector2 _bulletPosition;

        private Vector2 _bulletVelocity;

        // Константы для настройки
        public const float BULLET_SPEED = 500.0f; // Пикселей в секунду
        public const float MAX_LIFETIME = 3.0f; // Максимальное время жизни в секундах

        private float _lifeTime = 0f;
        private Vector2 _direction; // Нормализованный вектор направления

        public Vector2 Direction => _direction;

        // Физика
        private const float BULLET_VELOCITY = 5.0f;

        // Коллайдер
        private const int COLLIDER_WIDTH = 8 * 4;
        private const int COLLIDER_HEIGHT = 4 * 4;

        private readonly Rectangle _collider;

        private Sprite _sprite;

        public Vector2 GetPosition() => _bulletPosition;
        public Vector2 Velocity
        {
            get { return _bulletVelocity; }
            set { _bulletVelocity = value; }
        }

        public Vector2 GetColliderOffset() => new Vector2(_collider.X, _collider.Y);
        public Vector2 GetColliderSize() => new Vector2(_collider.Width, _collider.Height);
        public bool IsExpired { get; set; }

        public Bullet(Sprite sprite, Vector2 bulletPosition, Vector2 direction)
        {
            _sprite = sprite;
            _bulletPosition = bulletPosition;
            _direction = Vector2.Normalize(direction); // Нормализуем вектор
            _collider = new Rectangle(-COLLIDER_WIDTH / 2, -COLLIDER_HEIGHT / 2,
                                     COLLIDER_WIDTH, COLLIDER_HEIGHT);
        }

        public void Update(GameTime gameTime)
        {
            if (IsExpired) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Движение с учетом deltaTime
            _bulletPosition += _direction * BULLET_SPEED * deltaTime;

            // Увеличиваем время жизни
            _lifeTime += deltaTime;

            // Пуля исчезает после истечения времени жизни
            if (_lifeTime >= MAX_LIFETIME)
            {
                IsExpired = true;
            }
        }

        public void DrawDebug(SpriteBatch spriteBatch)
        {
            Rectangle bounds = GetBounds();

            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.Red });

            // Рисуем коллайдер
            Texture2D colliderTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            colliderTexture.SetData(new[] { Color.Red * 0.3f });
            spriteBatch.Draw(colliderTexture, bounds, Color.Red * 0.3f);

            // Рисуем контур коллайдера
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 1), Color.Red);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - 1, bounds.Width, 1), Color.Red);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, 1, bounds.Height), Color.Red);
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - 1, bounds.Y, 1, bounds.Height), Color.Red);

            pixel.Dispose();
            colliderTexture.Dispose();
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(
                (int)(_bulletPosition.X),
                (int)(_bulletPosition.Y),
                _collider.Width,
                _collider.Height
            );
        }

        public void Draw()
        {
            // Устанавливаем эффект отражения в зависимости от направления
            SpriteEffects effects = _direction.X < 0 ?
                SpriteEffects.FlipHorizontally : SpriteEffects.None;

            _sprite.Effects = effects;

            _sprite.Draw(Core.SpriteBatch, _bulletPosition);
        }

    }
}
