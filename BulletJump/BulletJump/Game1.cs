using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BulletJumpLibrary;
using BulletJump.Scenes;
using MonoGameGum;
using Gum.Forms;
using Gum.Forms.Controls;

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

            InitializeGum();

            ChangeScene(new MenuScene());
        }

        protected override void LoadContent()
        {
            // _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        private void InitializeGum()
        {
            // Инициализируем сервис Gum. Второй параметр определяет
            // версию используемых по умолчанию визуальных элементов. V2 - это последняя версия
            GumService.Default.Initialize(this, DefaultVisualsVersion.V2);

            // Сообщите сервису Gum, какой контент-менеджер использовать.  Мы расскажем ему
            // использовать глобальный контент-менеджер из нашего ядра.
            GumService.Default.ContentLoader.XnaContentManager = Core.Content;

            // Register keyboard input for UI control.
            FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);

            // Register gamepad input for Ui control.
            FrameworkElement.GamePadsForUiControl.AddRange(GumService.Default.Gamepads);
            // Настройте обратную навигацию по вкладкам пользовательского интерфейса таким образом, чтобы она также запускалась при нажатии на клавиатуру
            // Клавиша со стрелкой вверх.
            FrameworkElement.TabReverseKeyCombos.Add(
                new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Up });

            FrameworkElement.TabKeyCombos.Add(
                new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Down });

            // Ресурсы, созданные для пользовательского интерфейса, были увеличены на 1/4 размера,
            // чтобы сохранить размер атласа текстур небольшим.Итак, мы установим размер холста по умолчанию равным 1 / 4 размера
            // разрешения игры, а затем попросим gum увеличить масштаб в 4 раза.
            GumService.Default.CanvasWidth = 1280;
            GumService.Default.CanvasHeight = 720;
            GumService.Default.Renderer.Camera.Zoom = 1.0f;
        }
    }
}
