using BulletJump.Enums;
using BulletJumpLibrary;
using BulletJumpLibrary.Collisions;
using BulletJumpLibrary.Graphics;
using BulletJumpLibrary.Graphics.Animations;
using Gum.Graphics.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BulletJump.GameObjects
{
    public class Player : IPlayerCollidable
    {
        private Vector2 _playerPosition;
        private Vector2 _velocity;
        private GumAnimatedSprite _animatedSprite;
        private PlayerAnimationState _animationState;
        private bool _isMoving;
        private bool _isGrounded;
        private bool _wasGrounded;
        private SpriteEffects _spriteEffects;

        // Физика
        private const float MOVEMENT_SPEED = 5.0f;
        private const float JUMP_FORCE = -12.0f;
        private const float GRAVITY = 0.5f;
        private const float MAX_FALL_SPEED = 10.0f;

        // Коллайдер
        private const int COLLIDER_WIDTH = 42 * 4;
        private const int COLLIDER_HEIGHT = 41 * 4;
        private readonly Rectangle _collider;

        // Стрельба
        private List<Bullet> _bullets = new List<Bullet>();
        private const int MAX_BULLETS = 50;

        public IReadOnlyList<Bullet> GetBullets() => _bullets;
        public Sprite bulletTexture;

        public Player(AnimationChainList animationChains)
        {
            _animatedSprite = new GumAnimatedSprite(animationChains);
            _animatedSprite.Scale = new Vector2(4.0f, 4.0f);

            // Origin должен быть установлен в зависимости от размера спрайта
            // Пока установим в 0, потом обновим после загрузки анимации

            _collider = new Rectangle(-COLLIDER_WIDTH / 2, -COLLIDER_HEIGHT / 2, COLLIDER_WIDTH, COLLIDER_HEIGHT);
        }

        public void Initialize(Vector2 startingPosition, float stride)
        {
            _playerPosition = startingPosition;
            _velocity = Vector2.Zero;
            _isGrounded = true;
            _wasGrounded = true;
            _spriteEffects = SpriteEffects.None;

            // Устанавливаем начальную анимацию
            _animatedSprite.Play("Walk");
            _animatedSprite.Animate = false; // Не анимируем для Idle

            // Ключевое изменение: Origin должен быть в нижнем центре для платформера
            // Для простоты сначала установим в (0,0), чтобы позиция была в левом верхнем углу
            _animatedSprite.Origin = Vector2.Zero;

            // Устанавливаем позицию, учитывая смещение коллайдера
            // Позиция спрайта должна быть _playerPosition плюс смещение коллайдера
            _animatedSprite.Position = _playerPosition + new Vector2(_collider.X, _collider.Y);
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(
                (int)(_playerPosition.X + _collider.X),
                (int)(_playerPosition.Y + _collider.Y),
                _collider.Width,
                _collider.Height
            );
        }

        private void HandleInput()
        {
            _velocity.X = 0;
            bool isMovingNow = false;

            if (GameController.MoveLeft())
            {
                _velocity.X = -MOVEMENT_SPEED;
                isMovingNow = true;
                _spriteEffects = SpriteEffects.FlipHorizontally;
            }
            else if (GameController.MoveRight())
            {
                _velocity.X = MOVEMENT_SPEED;
                isMovingNow = true;
                _spriteEffects = SpriteEffects.None;
            }

            if (GameController.MoveUp() && _isGrounded)
            {
                Jump();
            }

            if (GameController.Shot() && _isGrounded)
            {
                Shot();
            }

            _isMoving = isMovingNow;
            _animatedSprite.Effects = _spriteEffects;
        }

        private void UpdateAnimation()
        {
            if (!_isGrounded)
            {
                if (_animationState != PlayerAnimationState.Jumping)
                {
                    _animatedSprite.Play("Jump");
                    _animatedSprite.Animate = true;
                    _animationState = PlayerAnimationState.Jumping;
                }
            }
            else if (_isMoving)
            {
                if (_animationState != PlayerAnimationState.Walking)
                {
                    _animatedSprite.Play("Walk");
                    _animatedSprite.Animate = true;
                    _animationState = PlayerAnimationState.Walking;
                }
            }
            else
            {
                if (_animationState != PlayerAnimationState.Idle)
                {
                    _animatedSprite.Play("Walk");
                    _animatedSprite.Animate = false;
                    _animatedSprite.ResetAnimation();
                    _animationState = PlayerAnimationState.Idle;
                }
            }
        }

        private void Jump()
        {
            if (_isGrounded)
            {
                _velocity.Y = JUMP_FORCE;
                _isGrounded = false;
            }
        }

        private void Shot()
        {
            if (_bullets.Count >= MAX_BULLETS) return;

            float offsetX = _spriteEffects == SpriteEffects.FlipHorizontally ? -60 : 40;
            var bullet = new Bullet(
                bulletTexture,
                new Vector2(_playerPosition.X + offsetX, _playerPosition.Y - 30),
                new Vector2(_spriteEffects == SpriteEffects.FlipHorizontally ? -1 : 1, 0));

            _bullets.Add(bullet);
        }

        public void ApplyPhysics()
        {
            if (!_isGrounded)
            {
                _velocity.Y += GRAVITY;
                _velocity.Y = Math.Min(_velocity.Y, MAX_FALL_SPEED);
            }
            else
            {
                _velocity.Y = 0;
            }

            _playerPosition += _velocity;
        }

        public void Update(GameTime gameTime)
        {
            HandleInput();
            ApplyPhysics();
            UpdateAnimation();

            // Обновляем позицию спрайта, учитывая смещение коллайдера
            _animatedSprite.Position = _playerPosition + new Vector2(_collider.X, _collider.Y);
            _animatedSprite.Update(gameTime);

            foreach (var bullet in _bullets)
            {
                bullet.Update(gameTime);
            }

            _wasGrounded = _isGrounded;
            _bullets.RemoveAll(x => x.IsExpired);
        }

        public void Draw()
        {
            _animatedSprite.Draw(Core.SpriteBatch);

            foreach (var bullet in _bullets)
            {
                if (!bullet.IsExpired)
                    bullet.Draw();
            }
        }

        public void ClearBullets() => _bullets.Clear();
        public Vector2 GetPosition() => _playerPosition;
        public void SetPosition(Vector2 newPosition) => _playerPosition = newPosition;
        public bool IsGrounded => _isGrounded;
        public bool WasGrounded => _wasGrounded;
        public Vector2 Velocity => _velocity;
        public PlayerAnimationState AnimationState => _animationState;
        public Vector2 GetColliderOffset() => new Vector2(_collider.X, _collider.Y);
        public Vector2 GetColliderSize() => new Vector2(_collider.Width, _collider.Height);

        public void SetGrounded(bool grounded)
        {
            if (_isGrounded != grounded)
            {
                _isGrounded = grounded;
                if (grounded) _velocity.Y = 0;
            }
        }

        public void SetVerticalVelocity(float velocityY) => _velocity.Y = velocityY;
    }
}