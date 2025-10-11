using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BulletJumpLibrary;
using BulletJump.Scenes;

namespace BulletJump
{
    public class Game1 : Core
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public Game1() : base("Bullet Jump", 1280, 720, false)
        {

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            ChangeScene(new GameScene());
        }

        protected override void LoadContent()
        {
            // _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        //protected override void Update(GameTime gameTime)
        //{
        //    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        //        Exit();

        //    // TODO: Add your update logic here

        //    base.Update(gameTime);
        //}

        //protected override void Draw(GameTime gameTime)
        //{
        //    GraphicsDevice.Clear(Color.CornflowerBlue);

        //    // TODO: Add your drawing code here

        //    base.Draw(gameTime);
        //}
    }
}
