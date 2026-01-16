using BulletJump.GameObjects;
using BulletJumpLibrary;
using BulletJumpLibrary.Collisions;
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
                CollisionManager.HandlePlayerCollision((IPlayerCollidable)_player, _tilemap);

                CollisionManager.HandleBulletCollision(_player.GetBullets().Cast<IBulletCollidable>(), _tilemap);

                // 3. Обновляем камеру
                if (_camera != null)
                {
                    _camera.Target = _player.GetPosition();
                    _camera.Update(gameTime);
                }
            }
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