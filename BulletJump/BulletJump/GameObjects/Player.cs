using BulletJumpLibrary;
using BulletJumpLibrary.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJump.GameObjects
{
    public class Player
    {
        private static readonly TimeSpan s_movementTime = TimeSpan.FromMilliseconds(200);

        private TimeSpan _movementTimer;

        private float _movementProgress;

        private AnimatedSprite _sprite;

        private Vector2 _playerPosition;

        private const float MOVEMENT_SPEED = 5.0f;

        private bool _wasMoving = false;

        private bool _isMoving = false;

        public Player(AnimatedSprite sprite)
        {
            _sprite = sprite;
        }

        public void Initialize(Vector2 startingPosition, float stride)
        {
            _movementTimer = TimeSpan.Zero;
            _playerPosition = Vector2.One;

            _sprite?.Stop();
        }

        private void HandleInput()
        {
            // Vector2 potentialNextDirection = Vector2.Zero;
            float speed = MOVEMENT_SPEED;

            bool isMovingNow = false;

            //if (GameController.MoveUp())
            //{
            //    potentialNextDirection = -Vector2.UnitY;
            //}
            //else if (GameController.MoveDown())
            //{
            //    potentialNextDirection = Vector2.UnitY;
            //}

            if (GameController.MoveLeft())
            {
                _playerPosition.X -= speed;
                isMovingNow = true;
                _sprite.Effects = SpriteEffects.FlipHorizontally;
            }
            else if (GameController.MoveRight())
            {
                _playerPosition.X += speed;
                isMovingNow = true;
                _sprite.Effects = SpriteEffects.None;
            }

            if (isMovingNow && !_wasMoving)
            {
                // Начали движение - запускаем анимацию
                _sprite?.Resume();
                if (_sprite.CurrentFrameIndex == 0)
                {
                    _sprite.SetFrame(1);
                }
            }
            else if (!isMovingNow && _wasMoving)
            {
                // Остановились - останавливаем анимацию на первом кадре
                _sprite?.Stop();
            }

            _wasMoving = isMovingNow;
            _isMoving = isMovingNow;



        }

        public void Update(GameTime gameTime)
        {
            HandleInput();

            if (_sprite != null)
            {
                _sprite.Update(gameTime);
            }
        }

        public void Draw()
        {
                _sprite.Draw(Core.SpriteBatch, _playerPosition);              
        }
    }
}
