using BulletJump.Enums;
using BulletJumpLibrary;
using BulletJumpLibrary.Graphics;
using BulletJumpLibrary.Graphics.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BulletJump.GameObjects
{
    public class Player
    {
        private Vector2 _playerPosition;
        private Vector2 _velocity;

        // Анимации
        private readonly IAnimationController _walkAnimation;
        private readonly IAnimationController _jumpAnimation;
        private IAnimationController _currentAnimation;

        // Состояния
        private PlayerAnimationState _animationState;
        private bool _isMoving;
        private bool _isGrounded;
        private bool _wasGrounded; // Для отслеживания предыдущего состояния
        private bool _isShooting;
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
        private const int MAX_BULLETS = 50; // Ограничение количества пуль

        public IReadOnlyList<Bullet> GetBullets() => _bullets;

        public Sprite bulletTexture;

        public Player(IAnimationController walkAnimation, IAnimationController jumpAnimation)
        {
            _walkAnimation = walkAnimation ?? throw new ArgumentNullException(nameof(walkAnimation));
            _jumpAnimation = jumpAnimation ?? throw new ArgumentNullException(nameof(jumpAnimation));
            _currentAnimation = _walkAnimation;
            _animationState = PlayerAnimationState.Idle;

            // Коллайдер центрирован относительно позиции спрайта
            _collider = new Rectangle(-COLLIDER_WIDTH / 2, -COLLIDER_HEIGHT / 2, COLLIDER_WIDTH, COLLIDER_HEIGHT);

            InitializeAnimations();
        }

        private void InitializeAnimations()
        {
            _walkAnimation.Stop();
            _jumpAnimation.Stop();
        }

        public void Initialize(Vector2 startingPosition, float stride)
        {
            _playerPosition = startingPosition;
            _velocity = Vector2.Zero;
            _isGrounded = true;
            _wasGrounded = true;
            _spriteEffects = SpriteEffects.None;

            SetAnimationState(PlayerAnimationState.Idle);
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
            _currentAnimation.SetEffects(_spriteEffects);
        }

        private void UpdateAnimationState()
        {
            PlayerAnimationState newState = DetermineAnimationState();

            // Переключаем состояние только если оно изменилось
            if (newState != _animationState)
            {
                SetAnimationState(newState);
            }
        }

        private PlayerAnimationState DetermineAnimationState()
        {
            if (!_isGrounded)
                return PlayerAnimationState.Jumping;

            return _isMoving ? PlayerAnimationState.Walking : PlayerAnimationState.Idle;
        }

        private void SetAnimationState(PlayerAnimationState newState)
        {
            if (newState == _animationState)
                return;

            _currentAnimation.Stop();

            _currentAnimation = newState switch
            {
                PlayerAnimationState.Jumping => _jumpAnimation,
                PlayerAnimationState.Walking => _walkAnimation,
                PlayerAnimationState.Idle => _walkAnimation,
                _ => _walkAnimation
            };

            switch (newState)
            {
                case PlayerAnimationState.Walking:
                    _currentAnimation.PlayImmediate();
                    break;
                case PlayerAnimationState.Jumping:
                    _currentAnimation.Play();
                    break;
                case PlayerAnimationState.Idle:
                    _currentAnimation.Stop();
                    break;
            }

            _animationState = newState;
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
            if (_bullets.Count >= MAX_BULLETS) return; // Ограничение

            // Рассчитываем позицию с учетом направления
            float offsetX = _spriteEffects == SpriteEffects.FlipHorizontally ? -60 : 40;
            var bullet = new Bullet(
                bulletTexture,
                new Vector2(_playerPosition.X + offsetX, _playerPosition.Y - 30),
                new Vector2(_spriteEffects == SpriteEffects.FlipHorizontally ? -1 : 1, 0));

            _bullets.Add(bullet);
        }

        public void ApplyPhysics()
        {
            // Применяем гравитацию только если не на земле
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
            UpdateAnimationState();
            _currentAnimation.Update(gameTime);
            // Обновляем пули с передачей GameTime
            foreach (var bullet in _bullets)
            {
                bullet.Update(gameTime);
            }

            _wasGrounded = _isGrounded;
            _bullets.RemoveAll(x => x.IsExpired);

            _wasGrounded = _isGrounded; // Сохраняем состояние для следующего кадра
        }

        Rectangle oldBulletBounds;
        public void Draw()
        {
            _currentAnimation.Draw(Core.SpriteBatch, _playerPosition);
            foreach (var bullet in _bullets)
            {
                if (!bullet.IsExpired)
                    bullet.Draw();
            }

            // DrawDebug(Core.SpriteBatch); // Раскомментируйте для отладки
        }

        public void ClearBullets()
        {
            _bullets.Clear();
        }

        public Vector2 GetPosition() => _playerPosition;
        public void SetPosition(Vector2 newPosition) => _playerPosition = newPosition;
        public bool IsGrounded => _isGrounded;
        public bool WasGrounded => _wasGrounded; // Новое свойство
        public Vector2 Velocity => _velocity;
        public PlayerAnimationState AnimationState => _animationState;
        public Vector2 GetColliderOffset() => new Vector2(_collider.X, _collider.Y);
        public Vector2 GetColliderSize() => new Vector2(_collider.Width, _collider.Height);

        public void SetBulletDirection()
        {

        }

        public void SetGrounded(bool grounded)
        {
            // Защита от быстрых переключений - только если состояние действительно изменилось
            if (_isGrounded != grounded)
            {
                _isGrounded = grounded;

                if (grounded)
                {
                    _velocity.Y = 0;
                }
            }
        }

        public void SetVerticalVelocity(float velocityY) => _velocity.Y = velocityY;

        // Отладочная отрисовка
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
    }
}