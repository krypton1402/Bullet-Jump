using BulletJump.GameObjects;
using BulletJumpLibrary;
using BulletJumpLibrary.Collisions;
using BulletJumpLibrary.Graphics;
using BulletJumpLibrary.Graphics.Animations;
using BulletJumpLibrary.Graphics.Interfaces;
using BulletJumpLibrary.Scenes;
using Gum.Graphics.Animation;
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

                // ВАЖНО: Используем центр коллайдера для позиционирования
                // Коллайдер имеет смещение (_collider.X, _collider.Y), которое равно (-ширина/2, -высота/2)
                // Поэтому позиция игрока (центр коллайдера) должна быть в середине тайла

                playerPos.X = tileX * _tilemap.TileWidth + _tilemap.TileWidth / 2;

                // Ставим игрока так, чтобы нижняя граница коллайдера была на верхней границе тайла
                // Коллайдер центрирован, поэтому его нижняя граница: playerPos.Y + _collider.Y + _collider.Height
                // Мы хотим, чтобы эта граница была на уровне: (tileY + 1) * _tilemap.TileHeight

                float bottomOfCollider = (tileY + 1) * _tilemap.TileHeight;
                float colliderBottomOffset = _player.GetColliderOffset().Y + _player.GetColliderSize().Y;
                playerPos.Y = bottomOfCollider - colliderBottomOffset;

                _player.Initialize(playerPos, 300);

                // Настраиваем камеру
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

                if (_tilemap.Layers.ContainsKey("Collision"))
                {
                    _tilemap.GetLayer("Collision").IsVisible = false;
                }

                // Создаем игрока с новой системой анимаций
                TextureAtlas playerAtlas = TextureAtlas.FromFile(Core.Content, "images/player-atlas-definition.xml");
                Sprite bullet = playerAtlas.CreateSprite("bullet-1");
                bullet.Scale = new Vector2(4.0f, 4.0f);

                // Создаем AnimationChain для каждой анимации
                var animationChains = CreatePlayerAnimationChains(playerAtlas);

                // Создаем игрока с AnimationChainList
                _player = new Player(animationChains);
                _player.bulletTexture = bullet;

                InitializeNewGame();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadContent: {ex.Message}");
            }
        }

        private AnimationChainList CreatePlayerAnimationChains(TextureAtlas atlas)
        {
            var animationChains = new AnimationChainList();

            // Создаем AnimationChain для ходьбы
            var walkAnimation = atlas.GetAnimation("player-animation");
            var walkChain = CreateAnimationChainFromAtlasAnimation(walkAnimation, "Walk");
            animationChains.Add(walkChain);

            // Создаем AnimationChain для прыжка
            var jumpAnimation = atlas.GetAnimation("player-jump-animation");
            var jumpChain = CreateAnimationChainFromAtlasAnimation(jumpAnimation, "Jump");
            animationChains.Add(jumpChain);

            return animationChains;
        }

        private AnimationChain CreateAnimationChainFromAtlasAnimation(Animation atlasAnimation, string chainName)
        {
            var chain = new AnimationChain
            {
                Name = chainName
            };

            foreach (var region in atlasAnimation.Frames)
            {
                var frame = new AnimationFrame
                {
                    TopCoordinate = region.TopTextureCoordinate,
                    BottomCoordinate = region.BottomTextureCoordinate,
                    LeftCoordinate = region.LeftTextureCoordinate,
                    RightCoordinate = region.RightTextureCoordinate,
                    FrameLength = (float)atlasAnimation.Delay.TotalSeconds,
                    Texture = region.Texture
                };
                chain.Add(frame);
            }

            return chain;
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