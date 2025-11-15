using BulletJumpLibrary.Audio;
using BulletJumpLibrary.Input;
using BulletJumpLibrary.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace BulletJumpLibrary
{
    public class Core : Game
    {

        internal static Core s_instance;

        /// <summary>
        /// Возвращает ссылку на основной экземпляр.
        /// </summary>
        public static Core Instance => s_instance;

        // Активная сцена
        private static Scene s_activeScene;

        // Следующая сцена для перехода, если она есть.
        private static Scene s_nextScene;

        /// <summary>
        /// Позволяет диспетчеру графических устройств управлять представлением графики.
        /// </summary>
        public static GraphicsDeviceManager Graphics { get; private set; }

        /// <summary>
        /// Возвращает графическое устройство, используемое для создания графических ресурсов и выполнения примитивного рендеринга.
        /// </summary>
        public static new GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Возвращает пакет спрайтов, используемый для всего 2D-рендеринга.
        /// </summary>
        public static SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// Возвращает контент-менеджер, используемый для загрузки глобальных ресурсов.
        /// </summary>
        public static new ContentManager Content { get; private set; }

        /// <summary>
        /// Gets a reference to the input management system.
        /// </summary>
        public static InputManager Input { get; private set; }

        /// <summary>
        /// Gets or Sets a value that indicates if the game should exit when the esc key on the keyboard is pressed.
        /// </summary>
        public static bool ExitOnEscape { get; set; }

        /// <summary>
        /// Gets a reference to the audio control system.
        /// </summary>
        public static AudioController Audio { get; private set; }

        /// <summary>
        /// Creates a new Core instance.
        /// </summary>
        /// <param name="title">Название, которое будет отображаться в строке заголовка игрового окна.</param>
        /// <param name="width">Начальная ширина игрового окна в пикселях.</param>
        /// <param name="height">Начальная высота игрового окна в пикселях.</param>
        /// <param name="fullScreen">Указывает, должна ли игра запускаться в полноэкранном режиме.</param>

        public Core(string title, int width, int height, bool fullScreen)
        {
            // Экземпляр должен быть толкьо один
            if (s_instance != null)
            {
                throw new InvalidOperationException($"Может быть создан только один базовый экземпляр");
            }
            // Сохранение ссылки на движок для глобального доступа участников.
            s_instance = this;

            // Создание нового GraphicManager
            Graphics = new GraphicsDeviceManager(this);

            //Установка стандартных свойств графики
            Graphics.PreferredBackBufferWidth = width;
            Graphics.PreferredBackBufferHeight = height;
            Graphics.IsFullScreen = fullScreen;

            //Применяем свойства
            Graphics.ApplyChanges();

            //Устанавливаем название окна
            Window.Title = title;

            //Устанавливаем для контент менеджера ссылку на базовый контент менеджер
            Content = base.Content;

            // Устанавливаем корневую директорию для контента
            Content.RootDirectory = "Content";

            // Видимость курсора мыши
            IsMouseVisible = true;

            // Exit on escape is true by default
            ExitOnEscape = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Установите графическое устройство ядра в соответствие с графическим устройством базовой игры
            //.
            GraphicsDevice = base.GraphicsDevice;

            // Create the sprite batch instance.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a new input manager.
            Input = new InputManager();

            // Create a new audio controller.
            Audio = new AudioController();
        }

        protected override void UnloadContent()
        {
            // Dispose of the audio controller.
            Audio.Dispose();

            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // Update the input manager.
            Input.Update(gameTime);

            // Update the audio controller.
            Audio.Update();

            if (ExitOnEscape && Input.Keyboard.WasKeyJustPressed(Keys.Escape))
            {
                Exit();
            }

            // if there is a next scene waiting to be switch to, then transition
            // to that scene.
            if (s_nextScene != null)
            {
                TransitionScene();
            }

            // If there is an active scene, update it.
            if (s_activeScene != null)
            {
                s_activeScene.Update(gameTime);
            }

            base.Update(gameTime);
        }

        //Переопределяем Draw
        protected override void Draw(GameTime gameTime)
        {
            // если активная сцена есть, то отрисовываем ее
            if (s_activeScene != null)
            {
                s_activeScene.Draw(gameTime);
            }

            base.Draw(gameTime);
        }

        public static void ChangeScene(Scene next)
        {
            // Only set the next scene value if it is not the same
            // instance as the currently active scene.
            if (s_activeScene != next)
            {
                s_nextScene = next;
            }
        }

        private static void TransitionScene()
        {
            // If there is an active scene, dispose of it.
            if (s_activeScene != null)
            {
                s_activeScene.Dispose();
            }

            // Force the garbage collector to collect to ensure memory is cleared.
            GC.Collect();

            // Change the currently active scene to the new scene.
            s_activeScene = s_nextScene;

            // Null out the next scene value so it does not trigger a change over and over.
            s_nextScene = null;

            // If the active scene now is not null, initialize it.
            // Remember, just like with Game, the Initialize call also calls the
            // Scene.LoadContent
            if (s_activeScene != null)
            {
                s_activeScene.Initialize();
            }
        }

        /// <summary>
        /// Конвертирует мировые координаты в координаты тайлов
        /// </summary>
        /// <param name="worldPosition">Мировые координаты</param>
        /// <param name="tileWidth">Ширина тайла</param>
        /// <param name="tileHeight">Высота тайла</param>
        /// <returns>Координаты тайла</returns>
        public static Point WorldToTile(Vector2 worldPosition, float tileWidth, float tileHeight)
        {
            return new Point(
                (int)(worldPosition.X / tileWidth),
                (int)(worldPosition.Y / tileHeight)
            );
        }

        /// <summary>
        /// Конвертирует координаты тайлов в мировые координаты
        /// </summary>
        /// <param name="tilePosition">Координаты тайла</param>
        /// <param name="tileWidth">Ширина тайла</param>
        /// <param name="tileHeight">Высота тайла</param>
        /// <returns>Мировые координаты</returns>
        public static Vector2 TileToWorld(Point tilePosition, float tileWidth, float tileHeight)
        {
            return new Vector2(
                tilePosition.X * tileWidth,
                tilePosition.Y * tileHeight
            );
        }

        /// <summary>
        /// Проверяет, находится ли точка внутри границ тайлмапа
        /// </summary>
        /// <param name="tilePosition">Позиция тайла</param>
        /// <param name="tilemapColumns">Количество колонок в тайлмапе</param>
        /// <param name="tilemapRows">Количество строк в тайлмапе</param>
        /// <returns>True если точка внутри границ</returns>
        public static bool IsInTilemapBounds(Point tilePosition, int tilemapColumns, int tilemapRows)
        {
            return tilePosition.X >= 0 && tilePosition.X < tilemapColumns &&
                   tilePosition.Y >= 0 && tilePosition.Y < tilemapRows;
        }
    }
}

