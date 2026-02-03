using BulletJumpLibrary.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Forms.Controls;
using BulletJumpLibrary.Graphics;
using BulletJumpLibrary;
using Microsoft.Xna.Framework.Input;
using MonoGameGum.GueDeriving;
using BulletJump.UI;

namespace BulletJump.Scenes
{
    public class MenuScene : Scene
    {
        private const string BULLET_TEXT = "BULLET";
        private const string JUMP_TEXT = "JUMP";
        private const string PRESS_ENTER_TEXT = "Press Enter To Start";

        // The font to use to render normal text.
        private SpriteFont _font;

        // The font used to render the title text.
        private SpriteFont _font5x;

        // The position to draw the dungeon text at.
        private Vector2 _bulletTextPos;

        // The origin to set for the dungeon text.
        private Vector2 _bulletTextOrigin;

        // The position to draw the slime text at.
        private Vector2 _jumpTextPos;

        // The origin to set for the slime text.
        private Vector2 _jumpTextOrigin;

        // The position to draw the press enter text at.
        private Vector2 _pressEnterPos;

        // The origin to set for the press enter text when drawing it.
        private Vector2 _pressEnterOrigin;

        // Фоновый паттерн
        private Texture2D _backgrounPattern;

        // Целевой прямоугольник, который будет заполнен фоновым рисунком.
        private Rectangle _backgroundDestination;

        private float _scrollSpeed = 50.0f;

        private Panel _titleScreenButtonsPanel;
        private Panel _optionsPanel;
        private AnimatedButton _optionsButton;
        private AnimatedButton _optionsBackButton;

        private TextureAtlas _textureAtlas;

        public override void Initialize()
        {
            base.Initialize();

            Vector2 size = _font5x.MeasureString(BULLET_TEXT);
            _bulletTextPos = new Vector2(640, 100);
            _bulletTextOrigin = size * 0.5f;

            size = _font5x.MeasureString(BULLET_TEXT);
            _jumpTextPos = new Vector2(710, 207);
            _jumpTextOrigin = size * 0.5f;

            _backgroundDestination = Core.GraphicsDevice.PresentationParameters.Bounds;

            InitializeUI();
        }

        public override void LoadContent()
        {
            _font = Core.Content.Load<SpriteFont>("fonts/drukwidecyr-bold");
            _font5x = Core.Content.Load<SpriteFont>("fonts/drukwidecyr-bold_5x");

            _textureAtlas = TextureAtlas.FromFile(Core.Content, "images/UI/menu-atlas-definition-1.xml");
        }

        public override void Update(GameTime gameTime)
        {
            // If the user presses enter, switch to the game scene.
            if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Enter))
            {
                Core.ChangeScene(new GameScene());
            }
            // Измените смещения для обтекания фонового рисунка, чтобы он
            // прокручивался вниз и вправо.

            //float offset = _scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            //_backgroundOffset.X -= offset;
            //_backgroundOffset.Y -= offset;
            //// Убедитесь, что смещения не выходят за границы текстуры, чтобы получилась
            //// бесшовная обертка.
            //_backgroundOffset.X %= _backgrounPattern.Width;
            //_backgroundOffset.Y %= _backgrounPattern.Height;

            GumService.Default.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

            //Core.SpriteBatch.Begin(samplerState: SamplerState.PointWrap);
            //Core.SpriteBatch.Draw(_backgrounPattern, _backgroundDestination, new Rectangle(_backgroundOffset.ToPoint(), _backgroundDestination.Size), Color.White * 0.5f);
            //Core.SpriteBatch.End();

            if (_titleScreenButtonsPanel.IsVisible)
            {

                Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

                Color dropShadowColor = Color.Black;

                Core.SpriteBatch.DrawString(_font5x, BULLET_TEXT, _bulletTextPos + new Vector2(5, 5), dropShadowColor, 0.0f, _bulletTextOrigin, 1.0f, SpriteEffects.None, 1.0f);


                Core.SpriteBatch.DrawString(_font5x, BULLET_TEXT, _bulletTextPos, Color.White, 0.0f, _bulletTextOrigin, 1.0f, SpriteEffects.None, 1.0f);


                Core.SpriteBatch.DrawString(_font5x, JUMP_TEXT, _jumpTextPos + new Vector2(5, 5), dropShadowColor, 0.0f, _jumpTextOrigin, 1.0f, SpriteEffects.None, 1.0f);


                Core.SpriteBatch.DrawString(_font5x, JUMP_TEXT, _jumpTextPos, Color.White, 0.0f, _jumpTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

                Core.SpriteBatch.End();
            }

            GumService.Default.Draw();
        }

        private void CreateTitlePanel()
        {

            // Create a container to hold all of our buttons
            _titleScreenButtonsPanel = new Panel();
            _titleScreenButtonsPanel.Dock(Gum.Wireframe.Dock.Fill);
            _titleScreenButtonsPanel.AddToRoot();

            var buttonContainer = new Panel();
            buttonContainer.Anchor(Gum.Wireframe.Anchor.Center);
            buttonContainer.Width = 150; // Ширина контейнера для кнопок
            buttonContainer.Height = 0; // Высота контейнера для кнопок
            buttonContainer.Y = 150;
            buttonContainer.Visual.ChildrenLayout = Gum.Managers.ChildrenLayout.TopToBottomStack;
            buttonContainer.Visual.StackSpacing = 20; // Расстояние между кнопками
            _titleScreenButtonsPanel.AddChild(buttonContainer);

            AnimatedButton startButton = new AnimatedButton(_textureAtlas, 0.5f);
            // buttonContainer.Anchor(Gum.Wireframe.Anchor.Left);
            startButton.X = 150;
            //startButton.Y = -120;
            startButton.Visual.Height = 70;
            startButton.Visual.Width = 128;
            startButton.Text = "Новая игра";
            startButton.Click += HandleStartClicked;

            buttonContainer.AddChild(startButton);

            _optionsButton = new AnimatedButton(_textureAtlas, 0.5f);
            //_optionsButton.Anchor(Gum.Wireframe.Anchor.Right);
            _optionsButton.X = 150;
            //_optionsButton.Y = -120;
            _optionsButton.Visual.Height = 70;
            _optionsButton.Visual.Width = 128;

            _optionsButton.Text = "НАСТРОЙКИ";
            _optionsButton.Click += HandleOptionsClicked;
            buttonContainer.AddChild(_optionsButton);
            startButton.IsFocused = true;
        }

        private void HandleStartClicked(object sender, EventArgs e)
        {
            // Core.Audio.PlaySoundEffect(_uiSoundEffect);

            Core.ChangeScene(new GameScene());
        }

        private void HandleOptionsClicked(object sender, EventArgs e)
        {
             // Core.Audio.PlaySoundEffect(_uiSoundEffect);

            _titleScreenButtonsPanel.IsVisible = false;

            _optionsButton.IsVisible = true;

            _optionsPanel.IsVisible = true;

            _optionsBackButton.IsFocused = true;
        }

        private void CreateOptionsPanel()
        {
            _optionsPanel = new Panel();
            _optionsPanel.Dock(Gum.Wireframe.Dock.Fill);
            _optionsPanel.IsVisible = false;
            _optionsPanel.AddToRoot();

            TextRuntime optionsText = new TextRuntime();
            optionsText.X = 10;
            optionsText.Y = 10;
            optionsText.Text = "OPTIONS";
            optionsText.UseCustomFont = true;
            optionsText.FontScale = 0.5f;
            optionsText.CustomFontFile = @"fonts/drukwidecyr-bold.fnt";
            _optionsPanel.AddChild(optionsText);

            Slider musicSlider = new Slider();
            musicSlider.Name = "MusicSlider";
            // musicSlider.Text = "MUSIC";
            musicSlider.Anchor(Gum.Wireframe.Anchor.Top);
            musicSlider.Visual.Y = 30f;
            musicSlider.Minimum = 0;
            musicSlider.Maximum = 1;
            musicSlider.Value = Core.Audio.SongVolume;
            musicSlider.SmallChange = .1;
            musicSlider.LargeChange = .2;
            musicSlider.ValueChanged += HandleMusicSliderValueChanged;
            musicSlider.ValueChangeCompleted += HandleMusicSliderValueChangeCompleted;
            _optionsPanel.AddChild(musicSlider);

            Slider sfxSlider = new Slider();
            sfxSlider.Name = "SfxSlider";
            // sfxSlider.Text = "SFX";
            sfxSlider.Anchor(Gum.Wireframe.Anchor.Top);
            sfxSlider.Visual.Y = 93;
            sfxSlider.Minimum = 0;
            sfxSlider.Maximum = 1;
            sfxSlider.Value = Core.Audio.SoundEffectVolume;
            sfxSlider.SmallChange = .1;
            sfxSlider.LargeChange = .2;
            sfxSlider.ValueChanged += HandleSfxSliderChanged;
            sfxSlider.ValueChangeCompleted += HandleSfxSliderChangeCompleted;
            _optionsPanel.AddChild(sfxSlider);

            _optionsBackButton = new AnimatedButton(_textureAtlas, 0.5f);
            _optionsBackButton.Text = "BACK";
            _optionsBackButton.Anchor(Gum.Wireframe.Anchor.BottomRight);
            _optionsBackButton.X = -28f;
            _optionsBackButton.Y = -10f;
            _optionsBackButton.Click += HandleOptionsButtonBack;
            _optionsPanel.AddChild(_optionsBackButton);
        }

        private void HandleOptionsButtonBack(object sender, EventArgs e)
        {
            // A UI interaction occurred, play the sound effect
            // Core.Audio.PlaySoundEffect(_uiSoundEffect);

            // Set the title panel to be visible.
            _titleScreenButtonsPanel.IsVisible = true;

            // Set the options panel to be invisible.
            _optionsPanel.IsVisible = false;

            // Give the options button on the title panel focus since we are coming
            // back from the options screen.
            _optionsButton.IsFocused = true;
        }

        private void HandleSfxSliderChanged(object sender, EventArgs e)
        {
            // Намеренно не воспроизводить звуковой эффект пользовательского интерфейса здесь, чтобы он не был
            // постоянно срабатывает, когда пользователь перемещает большой палец ползунка по дорожке


            // Get a reference to the sender as a Slider.
            var slider = (Slider)sender;

            // Set the global sound effect volume to the value of the slider.;
            Core.Audio.SoundEffectVolume = (float)slider.Value;
        }

        private void HandleSfxSliderChangeCompleted(object sender, EventArgs e)
        {
            // Core.Audio.PlaySoundEffect(_uiSoundEffect);
        }

        private void HandleMusicSliderValueChanged(object sender, EventArgs e)
        {
            var slider = (Slider)sender;


            Core.Audio.SongVolume = (float)slider.Value;
        }
        private void HandleMusicSliderValueChangeCompleted(object sender, EventArgs args)
        {
            // A UI interaction occurred, play the sound effect
            // Core.Audio.PlaySoundEffect(_uiSoundEffect);
        }

        private void InitializeUI()
        {

            GumService.Default.Root.Children.Clear();

            CreateTitlePanel();
            CreateOptionsPanel();
        }
    }
}
