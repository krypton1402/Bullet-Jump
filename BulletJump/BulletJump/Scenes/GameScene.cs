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

        private Sprite _stayPlayer;

        private Tilemap _tilemap;

        private Rectangle _roomBounds;

        public override void Initialize()
        {
            base.Initialize();

            Core.ExitOnEscape = false;

            InitializeNewGame();
        }

        private void InitializeNewGame()
        {
            Vector2 playerPos = new Vector2();

            _roomBounds = Core.GraphicsDevice.PresentationParameters.Bounds;
            _roomBounds.Inflate(-_tilemap.TileWidth, -_tilemap.TileHeight);


            // Позиция на последнем тайле в правом нижнем углу
            int tileX = 3; // Предпоследний тайл для лучшего вида
            int tileY = _tilemap.Rows - 1;    // Последний ряд

            playerPos.X = tileX * _tilemap.TileWidth + (_tilemap.TileWidth - _player.GetBounds().Width) / 2;
            playerPos.Y = tileY * _tilemap.TileHeight - _player.GetBounds().Height + _tilemap.TileHeight - 36;

            Rectangle movementBounds = new Rectangle(
                0,
                0,
                (int)(_tilemap.Columns * _tilemap.TileWidth),
                (int)(_tilemap.Rows * _tilemap.TileHeight)
            );

            _player.Initialize(playerPos, 300);

        }

        public override void LoadContent()
        {
            // Create the texture atlas from the XML configuration file.
            TextureAtlas playerAtlas = TextureAtlas.FromFile(Core.Content, "images/player-atlas-definition.xml");

            _tilemap = Tilemap.FromFile(Content, "images/enviroment-atlas-definition.xml");
            _tilemap.Scale = new Vector2(5.0f, 5.0f);

            //// Create the tilemap from the XML configuration file.
            //_tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
            //_tilemap.Scale = new Vector2(4.0f, 4.0f);

            // Create the animated player for the player from the atlas.
            AnimatedSprite playerWalkAnimation = playerAtlas.CreateAnimatedSprite("player-animation");
            AnimatedSprite playerJumpAnimation = playerAtlas.CreateAnimatedSprite("player-jump-animation");

            playerWalkAnimation.Scale = new Vector2(4.0f, 4.0f);
            playerJumpAnimation.Scale = new Vector2(4.0f, 4.0f);

            IAnimationController walkAnimation = new AnimationController(playerWalkAnimation);
            IAnimationController jumpAnimation = new AnimationController(playerJumpAnimation);

            // Create the slime.
            _player = new Player(walkAnimation, jumpAnimation);

        }

        public override void Update(GameTime gameTime)
        {

            _player.Update(gameTime);
            CollisionChecks();

        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);



            _tilemap.Draw(Core.SpriteBatch);

            _player.Draw();

            // _bat.Draw();

            Core.SpriteBatch.End();

            // _ui.Draw();
        }

        public void CollisionChecks()
        {
            Rectangle playerBounds = _player.GetBounds();

            Vector2 playerPos = _player.GetPosition();

            if (playerBounds.Left < _roomBounds.Left)
            {
                playerPos.X += _roomBounds.Left - playerBounds.Left;
            }
            else if (playerBounds.Right > _roomBounds.Right)
            {
                playerPos.X -= playerBounds.Right - _roomBounds.Right;
            }

            //if (playerBounds.Top < _roomBounds.Top)
            //{
            //    playerPos.Y += _roomBounds.Top - playerBounds.Top;
            //}
            //else if (playerBounds.Bottom > _roomBounds.Bottom)
            //{
            //    playerPos.Y -= playerBounds.Bottom - _roomBounds.Bottom;
            //}

            // Устанавливаем скорректированную позицию
            _player.SetPosition(playerPos);
        }
    }
}
