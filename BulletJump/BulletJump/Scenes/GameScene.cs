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
                int tileX = 3;
                int tileY = _tilemap.Rows - 1;

                playerPos.X = tileX * _tilemap.TileWidth + (_tilemap.TileWidth - _player.GetBounds().Width) / 2;
                playerPos.Y = tileY * _tilemap.TileHeight - _player.GetBounds().Height + _tilemap.TileHeight - 36;

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

        private Point FindSafeSpawnPosition()
        {
            // Ищем первую безопасную позицию на слое Ground
            for (int y = 0; y < _tilemap.Rows; y++)
            {
                for (int x = 0; x < _tilemap.Columns; x++)
                {
                    if (!_tilemap.IsTileEmpty("Ground", x, y) &&
                        _tilemap.IsTileEmpty("Collision", x, y))
                    {
                        return new Point(x, y);
                    }
                }
            }

            return new Point(_tilemap.Columns / 2, _tilemap.Rows / 2);
        }

        public override void LoadContent()
        {
            try
            {
                // Загружаем тайлмап
                _tilemap = Tilemap.FromFile(Content, "images/enviroment-atlas-definition.xml");
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

                playerWalkAnimation.Scale = new Vector2(4.0f, 4.0f);
                playerJumpAnimation.Scale = new Vector2(4.0f, 4.0f);

                IAnimationController walkAnimation = new AnimationController(playerWalkAnimation);
                IAnimationController jumpAnimation = new AnimationController(playerJumpAnimation);

                _player = new Player(walkAnimation, jumpAnimation);

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
                _player.Update(gameTime);

                // Обновляем цель камеры - позицию игрока
                if (_camera != null)
                {
                    _camera.Target = _player.GetPosition();
                    _camera.Update(gameTime);
                }

                HandleCollisions();
            }
        }

        private void HandleCollisions()
        {
            if (!_isInitialized) return;

            Rectangle playerBounds = _player.GetBounds();
            Point playerTile = Core.WorldToTile(_player.GetPosition(), _tilemap.TileWidth, _tilemap.TileHeight);

            CheckTileCollision(playerBounds, playerTile);
        }

        private void CheckTileCollision(Rectangle playerBounds, Point playerTile)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Point checkTile = new Point(playerTile.X + x, playerTile.Y + y);

                    if (Core.IsInTilemapBounds(checkTile, _tilemap.Columns, _tilemap.Rows))
                    {
                        if (!_tilemap.IsTileEmpty("Collision", checkTile.X, checkTile.Y))
                        {
                            HandleTileCollision(playerBounds, checkTile);
                        }
                    }
                }
            }
        }

        private void HandleTileCollision(Rectangle playerBounds, Point collisionTile)
        {
            Vector2 tileWorldPos = Core.TileToWorld(collisionTile, _tilemap.TileWidth, _tilemap.TileHeight);
            Rectangle tileBounds = new Rectangle(
                (int)tileWorldPos.X,
                (int)tileWorldPos.Y,
                (int)_tilemap.TileWidth,
                (int)_tilemap.TileHeight
            );

            if (playerBounds.Intersects(tileBounds))
            {
                Vector2 correction = CalculateCollisionCorrection(playerBounds, tileBounds);
                _player.SetPosition(_player.GetPosition() + correction);
            }
        }

        private Vector2 CalculateCollisionCorrection(Rectangle player, Rectangle tile)
        {
            Vector2 correction = Vector2.Zero;

            float overlapLeft = player.Right - tile.Left;
            float overlapRight = tile.Right - player.Left;
            float overlapTop = player.Bottom - tile.Top;
            float overlapBottom = tile.Bottom - player.Top;

            float minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), Math.Min(overlapTop, overlapBottom));

            if (minOverlap == overlapLeft)
                correction.X = -overlapLeft;
            else if (minOverlap == overlapRight)
                correction.X = overlapRight;
            else if (minOverlap == overlapTop)
                correction.Y = -overlapTop;
            else if (minOverlap == overlapBottom)
                correction.Y = overlapBottom;

            return correction;
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.Black);

            if (!_isInitialized || _camera == null)
            {
                // Если камера не готова, рисуем без трансформации
                Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
                Core.SpriteBatch.End();
                return;
            }

            // Используем матрицу трансформации камеры
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
