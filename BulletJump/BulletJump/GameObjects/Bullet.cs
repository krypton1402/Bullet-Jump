using BulletJump.Enums;
using BulletJumpLibrary;
using BulletJumpLibrary.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJump.GameObjects
{
    public class Bullet
    {
        public Vector2 _bulletPosition;

        private Vector2 _bulletVelocity;

        // Физика
        private const float BULLET_VELOCITY = 5.0f;

        // Коллайдер
        private const int COLLIDER_WIDTH = 8 * 4;
        private const int COLLIDER_HEIGHT = 4 * 4;

        private readonly Rectangle _collider;

        private Sprite _sprite;

        public Vector2 GetPosition() => _bulletPosition;
        public Vector2 Velocity => _bulletVelocity;
        public Vector2 GetColliderOffset() => new Vector2(_collider.X, _collider.Y);
        public Vector2 GetColliderSize() => new Vector2(_collider.Width, _collider.Height);

        public Bullet(Sprite sprite, Vector2 bulletPosition, Vector2 bulletVelocity)
        {
            _sprite = sprite;

            _bulletVelocity = bulletVelocity;

            _bulletPosition = bulletPosition;

            // Коллайдер центрирован относительно позиции спрайта
            _collider = new Rectangle(-COLLIDER_WIDTH / 2, -COLLIDER_HEIGHT / 2, COLLIDER_WIDTH, COLLIDER_HEIGHT);
        }

        public void Update()
        {
            _bulletPosition.X += _bulletVelocity.X;
        }

        public void Draw()
        {
            _sprite.Draw(Core.SpriteBatch, _bulletPosition);
        }

    }
}
