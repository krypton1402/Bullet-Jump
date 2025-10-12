using BulletJump.GameObjects;
using BulletJumpLibrary;
using BulletJumpLibrary.Graphics;
using BulletJumpLibrary.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public override void Initialize()
        {
            base.Initialize();

            Core.ExitOnEscape = false;

            InitializeNewGame();
        }

        private void InitializeNewGame()
        {
            Vector2 playerPos = new Vector2();

            playerPos.X = 0;
            playerPos.Y = 0;
            //playerPos.X = (_tilemap.Columns / 2) * _tilemap.TileWidth;
            //playerPos.Y = (_tilemap.Rows / 2) * _tilemap.TileHeight;

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
            AnimatedSprite playerAnimation = playerAtlas.CreateAnimatedSprite("player-animation");

            playerAnimation.Scale = new Vector2(4.0f, 4.0f);

            // Create the slime.
            _player = new Player(playerAnimation);

        }

        public override void Update(GameTime gameTime)
        {

            _player.Update(gameTime);

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
    }
}
