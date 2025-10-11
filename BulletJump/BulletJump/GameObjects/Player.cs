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
    public class Player
    {
        private static readonly TimeSpan s_movementTime = TimeSpan.FromMilliseconds(200);

        private TimeSpan _movementTimer;

        private float _movementProgress;

        private AnimatedSprite _sprite;

        public Player(AnimatedSprite sprite)
        {
            _sprite = sprite;
        }

        public void Initialize(Vector2 startingPosition, float stride)
        {
            _movementTimer = TimeSpan.Zero;

        }

        private void HandleInput()
        {
            Vector2 potentialNextDirection = Vector2.Zero;

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
                potentialNextDirection = -Vector2.UnitX;
            }
            else if (GameController.MoveRight())
            {
                potentialNextDirection = Vector2.UnitX;
            }
            

        }

        public void Update(GameTime gameTime)
        {
            _sprite.Update(gameTime);

            HandleInput();

        }

        public void Draw()
        {
                // Вычислить визуальное положение сегмента в данный момент,
                // перемещаясь между его позициями "в" и "до" с помощью перемещения
                // величина смещения lerp
                Vector2 pos = Vector2.Zero;
                // Нарисуйте слайм-спрайта в рассчитанном визуальном положении этого сегмента
                //
                _sprite.Draw(Core.SpriteBatch, pos);
        }
    }
}
