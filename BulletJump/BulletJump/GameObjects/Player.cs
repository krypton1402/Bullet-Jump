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

        public Player(AnimatedSprite sprite)
        {
            _sprite = sprite;
        }

        public void Initialize(Vector2 startingPosition, float stride)
        {
            _movementTimer = TimeSpan.Zero;
            _playerPosition = Vector2.One;

        }

        private void HandleInput()
        {
            // Vector2 potentialNextDirection = Vector2.Zero;
            float speed = MOVEMENT_SPEED;

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
            }
            else if (GameController.MoveRight())
            {
                _playerPosition.X += speed;
            }
            

        }

        public void Update(GameTime gameTime)
        {
            if (_sprite != null)
            {
                _sprite.Update(gameTime);
            }

            HandleInput();

        }

        public void Draw()
        {
            if (!GameController.MoveLeft() && !GameController.MoveRight())
            {
                _sprite.Animation.Frames[0].Draw(Core.SpriteBatch, _playerPosition, Color.White, 0.0f, Vector2.Zero, 4.0f, SpriteEffects.None, 1.0f);
            }
            else
            {
                _sprite.Draw(Core.SpriteBatch, _playerPosition);
            }
                
        }
    }
}
