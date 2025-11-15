using BulletJump.GameObjects;
using BulletJumpLibrary;
using BulletJumpLibrary.Graphics;
using BulletJumpLibrary.Graphics.Interfaces;
using BulletJumpLibrary.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace BulletJump.Scenes
{
    public class GameScene : Scene
    {
        private enum GameState
        {
            Playing,
            Paused,
            GameOver
        }

        private Player _player;
        private Tilemap _tilemap;
        private Camera _camera;
        private GameState _currentState;
        private bool _isInitialized = false;

        public override void Initialize()
        {
            base.Initialize();

            Core.ExitOnEscape = false;
            _currentState = GameState.Playing;
        }

        private void InitializeNewGame()
        {
            try
            {
                Vector2 playerPos = new Vector2();

                // Позиция на последнем тайле в левом нижнем углу
                int tileX = 7;
                int tileY = _tilemap.Rows - 10;

                playerPos.X = tileX * _tilemap.TileWidth + (_tilemap.TileWidth - _player.GetBounds().Width) / 2;
                playerPos.Y = tileY * _tilemap.TileHeight - _player.GetBounds().Height + _tilemap.TileHeight;

                _player.Initialize(playerPos, 300);

                // Создаем и настраиваем камеру ПОСЛЕ создания игрока и тайлмапа
                _camera = new Camera(Core.GraphicsDevice.Viewport);
                _camera.Smoothness = 0.1f;
                _camera.SetBoundsFromTilemap(_tilemap);
                _camera.Position = playerPos;
                _camera.Target = playerPos;

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in InitializeNewGame: {ex.Message}");
            }
        }

        public override void LoadContent()
        {
            try
            {
                // Загружаем тайлмап
                _tilemap = Tilemap.FromFile(Content, "images/level1-enviroment-atlas-definition.xml");
                _tilemap.Scale = new Vector2(5.0f, 5.0f);

                // Настраиваем видимость слоев
                if (_tilemap.Layers.ContainsKey("Collision"))
                {
                    _tilemap.GetLayer("Collision").IsVisible = false;
                }

                // Создаем игрока
                TextureAtlas playerAtlas = TextureAtlas.FromFile(Core.Content, "images/player-atlas-definition.xml");

                AnimatedSprite playerWalkAnimation = playerAtlas.CreateAnimatedSprite("player-animation");
                AnimatedSprite playerJumpAnimation = playerAtlas.CreateAnimatedSprite("player-jump-animation");
                Sprite bullet = playerAtlas.CreateSprite("bullet-1");
                bullet.Scale = new Vector2(4.0f, 4.0f);

                playerWalkAnimation.Scale = new Vector2(4.0f, 4.0f);
                playerJumpAnimation.Scale = new Vector2(4.0f, 4.0f);

                // УСТАНАВЛИВАЕМ ORIGIN В ЦЕНТРЕ ДЛЯ ОБЕИХ АНИМАЦИЙ
                playerWalkAnimation.CenterOrigin();
                playerJumpAnimation.CenterOrigin();

                IAnimationController walkAnimation = new AnimationController(playerWalkAnimation);
                IAnimationController jumpAnimation = new AnimationController(playerJumpAnimation);

                _player = new Player(walkAnimation, jumpAnimation);
                _player.bulletTexture = bullet;

                // Инициализируем игру ПОСЛЕ загрузки всего контента
                InitializeNewGame();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadContent: {ex.Message}");
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (_currentState == GameState.Playing && _isInitialized)
            {
                // 1. Сначала обновляем игрока (ввод, физика, анимация)
                _player.Update(gameTime);

                // 2. Обрабатываем коллизии
                HandleCollisions();

                // 3. Обновляем камеру
                if (_camera != null)
                {
                    _camera.Target = _player.GetPosition();
                    _camera.Update(gameTime);
                }
            }
        }

        private void HandleCollisions()
        {
            if (!_isInitialized) return;

            Rectangle playerBounds = _player.GetBounds();
            Point playerTile = Core.WorldToTile(_player.GetPosition(), _tilemap.TileWidth, _tilemap.TileHeight);

            bool wasGroundedThisFrame = false;
            bool hadAnyCollision = false;

            // Проверяем коллизии со всеми окружающими тайлами
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Point checkTile = new Point(playerTile.X + x, playerTile.Y + y);

                    if (Core.IsInTilemapBounds(checkTile, _tilemap.Columns, _tilemap.Rows))
                    {
                        if (!_tilemap.IsTileEmpty("Collision", checkTile.X, checkTile.Y))
                        {
                            hadAnyCollision = true;
                            if (HandleTileCollision(playerBounds, checkTile))
                            {
                                wasGroundedThisFrame = true;
                            }
                        }
                    }
                }
            }

            // Если не было обычных коллизий с землей, но игрок движется вниз - проверяем землю под ногами
            if (!wasGroundedThisFrame && _player.Velocity.Y >= 0)
            {
                wasGroundedThisFrame = CheckGroundBelow();
            }

            // Стабильно устанавливаем состояние grounded
            _player.SetGrounded(wasGroundedThisFrame);
        }

        private bool CheckGroundBelow()
        {
            Rectangle bounds = _player.GetBounds();

            // Проверяем несколько точек под ногами для большей стабильности
            Vector2[] checkPoints = {
        new Vector2(bounds.Left + 5, bounds.Bottom + 2),
        new Vector2(bounds.Center.X, bounds.Bottom + 2),
        new Vector2(bounds.Right - 5, bounds.Bottom + 2)
    };

            bool foundGround = false;

            foreach (Vector2 checkPoint in checkPoints)
            {
                Point tileBelow = Core.WorldToTile(checkPoint, _tilemap.TileWidth, _tilemap.TileHeight);

                if (Core.IsInTilemapBounds(tileBelow, _tilemap.Columns, _tilemap.Rows))
                {
                    if (!_tilemap.IsTileEmpty("Collision", tileBelow.X, tileBelow.Y))
                    {
                        foundGround = true;

                        // Корректируем позицию только если действительно нужно
                        if (ShouldCorrectPosition(tileBelow))
                        {
                            Vector2 tileWorldPos = Core.TileToWorld(tileBelow, _tilemap.TileWidth, _tilemap.TileHeight);
                            float groundY = tileWorldPos.Y - _player.GetColliderSize().Y - _player.GetColliderOffset().Y;
                            _player.SetPosition(new Vector2(_player.GetPosition().X, groundY));
                        }
                        break; // Достаточно найти один тайл земли
                    }
                }
            }

            return foundGround;
        }

        private bool ShouldCorrectPosition(Point tileBelow)
        {
            // Корректируем позицию только если игрок действительно падает на платформу
            Rectangle bounds = _player.GetBounds();
            Vector2 tileWorldPos = Core.TileToWorld(tileBelow, _tilemap.TileWidth, _tilemap.TileHeight);
            float distanceToGround = tileWorldPos.Y - bounds.Bottom;

            // Корректируем только если расстояние небольшое (игрок близко к земле)
            return distanceToGround >= 0 && distanceToGround < 10f;
        }

        private bool HandleTileCollision(Rectangle playerBounds, Point collisionTile)
        {
            Vector2 tileWorldPos = Core.TileToWorld(collisionTile, _tilemap.TileWidth, _tilemap.TileHeight);
            Rectangle tileBounds = new Rectangle(
                (int)tileWorldPos.X,
                (int)tileWorldPos.Y,
                (int)_tilemap.TileWidth,
                (int)_tilemap.TileHeight
            );

            if (!playerBounds.Intersects(tileBounds))
                return false;

            Vector2 correction = CalculateCollisionCorrection(playerBounds, tileBounds);

            // Применяем коррекцию позиции игрока
            Vector2 newPosition = _player.GetPosition() + correction;
            _player.SetPosition(newPosition);

            // Определяем тип столкновения
            if (correction.Y < 0) // Столкновение сверху (игрок стоит на тайле)
            {
                _player.SetVerticalVelocity(0);
                return true; // Это коллизия с землей
            }
            else if (correction.Y > 0) // Столкновение снизу (удар головой)
            {
                _player.SetVerticalVelocity(0);
            }

            return false;
        }


        private Vector2 CalculateCollisionCorrection(Rectangle player, Rectangle tile)
        {
            Vector2 correction = Vector2.Zero;

            // Рассчитываем глубину проникновения с каждой стороны
            float overlapLeft = player.Right - tile.Left;
            float overlapRight = tile.Right - player.Left;
            float overlapTop = player.Bottom - tile.Top;
            float overlapBottom = tile.Bottom - player.Top;

            // Находим минимальное перекрытие
            float minOverlap = float.MaxValue;
            Vector2 minCorrection = Vector2.Zero;

            if (overlapLeft > 0 && overlapLeft < minOverlap)
            {
                minOverlap = overlapLeft;
                minCorrection = new Vector2(-overlapLeft, 0);
            }

            if (overlapRight > 0 && overlapRight < minOverlap)
            {
                minOverlap = overlapRight;
                minCorrection = new Vector2(overlapRight, 0);
            }

            if (overlapTop > 0 && overlapTop < minOverlap)
            {
                minOverlap = overlapTop;
                minCorrection = new Vector2(0, -overlapTop);
            }

            if (overlapBottom > 0 && overlapBottom < minOverlap)
            {
                minOverlap = overlapBottom;
                minCorrection = new Vector2(0, overlapBottom);
            }

            return minCorrection;
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);

            if (!_isInitialized || _camera == null)
            {
                Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
                Core.SpriteBatch.End();
                return;
            }

            Core.SpriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: _camera.GetTransformMatrix()
            );

            _tilemap.Draw(Core.SpriteBatch);
            _player.Draw();

            Core.SpriteBatch.End();
        }
    }
}