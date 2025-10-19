using BulletJump.Enums;
using BulletJumpLibrary;
using BulletJumpLibrary.Graphics;
using BulletJumpLibrary.Graphics.Interfaces;
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
        private SpriteEffects _spriteEffects;

        //Физика
        private const float MOVEMENT_SPEED = 5.0f;
        private const float JUMP_FORCE = -12.0f; // Отрицательное значение - прыжок вверх
        private const float GRAVITY = 0.5f;
        private float _groundLevel;     

        public Player(IAnimationController walkAnimation, IAnimationController jumpAnimation)
        {
            _walkAnimation = walkAnimation ?? throw new ArgumentNullException(nameof(walkAnimation));
            _jumpAnimation = jumpAnimation ?? throw new ArgumentNullException(nameof(jumpAnimation));
            _currentAnimation = _walkAnimation;
            _animationState = PlayerAnimationState.Idle;

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
            _groundLevel = _playerPosition.Y;
            _spriteEffects = SpriteEffects.None;

            SetAnimationState(PlayerAnimationState.Idle);
        }

        public Rectangle GetBounds()
        {
            if (_currentAnimation == null)
                return Rectangle.Empty;

            Vector2 position = _playerPosition;
            Vector2 origin = _currentAnimation.Origin;
            Vector2 scale = _currentAnimation.Scale;

            float width = _currentAnimation.Width;
            float height = _currentAnimation.Height;

            // Вычисляем фактическую позицию с учетом origin
            float x = position.X - origin.X * scale.X;
            float y = position.Y - origin.Y * scale.Y;

            return new Rectangle((int)x, (int)y, (int)width, (int)height);
        }

        // Для отладки - визуализация bounds
        public void DrawDebug(SpriteBatch spriteBatch)
        {
            Rectangle bounds = GetBounds();

            // Рисуем красную рамку вокруг bounds
            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.Red });

            // Верхняя линия
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 1), Color.Red);
            // Нижняя линия
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y + bounds.Height, bounds.Width, 1), Color.Red);
            // Левая линия
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, 1, bounds.Height), Color.Red);
            // Правая линия
            spriteBatch.Draw(pixel, new Rectangle(bounds.X + bounds.Width, bounds.Y, 1, bounds.Height), Color.Red);
        }

        private void HandleInput()
        {
            float speed = MOVEMENT_SPEED;
            bool isMovingNow = false;

            if (GameController.MoveLeft())
            {
                _playerPosition.X -= speed;
                isMovingNow = true;
                _spriteEffects = SpriteEffects.FlipHorizontally;
            }
            else if (GameController.MoveRight())
            {
                _playerPosition.X += speed;
                isMovingNow = true;
                _spriteEffects = SpriteEffects.None;
            }

            if (GameController.MoveUp())
            {
                Jump();
            }

            // Обновляем состояние движения и анимации
            _isMoving = isMovingNow;
            UpdateAnimationState();

            _currentAnimation.SetEffects(_spriteEffects);
        }

        private void UpdateAnimationState()
        {
            PlayerAnimationState newState;

            if (!_isGrounded)
            {
                newState = PlayerAnimationState.Jumping;
            }
            else if (_isMoving)
            {
                newState = PlayerAnimationState.Walking;
            }
            else
            {
                newState = PlayerAnimationState.Idle;
            }

            // Переключаем состояние только если оно изменилось
            if (newState != _animationState)
            {
                SetAnimationState(newState);
            }
            // Если продолжаем движение, убеждаемся что анимация воспроизводится
            else if (newState == PlayerAnimationState.Walking)
            {
                var animationController = _currentAnimation as AnimationController;
                if (animationController != null && !animationController.IsPlaying)
                {
                    // Используем немедленный запуск для избежания задержки
                    _currentAnimation.PlayImmediate();
                }
            }
        }

        private void SetAnimationState(PlayerAnimationState newState)
        {
            // Останавливаем предыдущую анимацию
            _currentAnimation.Stop();

            // Выбираем новую анимацию
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
                    // Используем немедленный запуск для ходьбы
                    _currentAnimation.PlayImmediate();
                    break;
                case PlayerAnimationState.Jumping:
                    // Для прыжка можно использовать обычный запуск
                    _currentAnimation.Play();
                    break;
                case PlayerAnimationState.Idle:
                    // Для Idle останавливаем на первом кадре
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

        private void ApplyPhysics()
        {
            // Применяем гравитацию
            if (!_isGrounded)
            {
                _velocity.Y += GRAVITY;
            }

            // Обновляем позицию
            _playerPosition += _velocity;

            // Проверка приземления
            if (_playerPosition.Y >= _groundLevel)
            {
                _playerPosition.Y = _groundLevel;
                _velocity.Y = 0;
                _isGrounded = true;

                // Если не двигаемся, останавливаем анимацию при приземлении
                if (!_isMoving)
                {
                    _currentAnimation?.Stop();
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            HandleInput();
            ApplyPhysics();

            _currentAnimation.Update(gameTime);
        }

        public void Draw()
        {
                _currentAnimation.Draw(Core.SpriteBatch, _playerPosition);
            //DrawDebug(Core.SpriteBatch);
        }

        public void StayInRoom()
        {

        }

        // Для проверки столкновений
        public bool Intersects(Rectangle otherBounds)
        {
            return GetBounds().Intersects(otherBounds);
        }

        public bool Intersects(Player otherPlayer)
        {
            return GetBounds().Intersects(otherPlayer.GetBounds());
        }

        // Метод для получения текущей позиции
        public Vector2 GetPosition()
        {
            return _playerPosition;
        }

        // Метод для установки позиции
        public void SetPosition(Vector2 newPosition)
        {
            _playerPosition = newPosition;
        }

        // Свойства для доступа к состоянию
        public bool IsGrounded => _isGrounded;
        public bool IsJumping => !_isGrounded;
        public Vector2 Velocity => _velocity;
        public PlayerAnimationState AnimationState => _animationState;
    }
}
