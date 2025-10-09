using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJumpLibrary.Scenes
{
    public abstract class Scene : IDisposable
    {
        /// <summary>
        /// Возвращает ContentManager, используемый для загрузки ресурсов, относящихся к конкретной сцене.
        /// </summary>
        /// <remarks>
        /// Ресурсы, загруженные через этот ContentManager, будут автоматически выгружены после завершения этой сцены.
        /// </remarks>
        protected ContentManager Content { get; }

        /// <summary>
        /// Возвращает значение, указывающее, была ли удалена сцена.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Создает новый экземпляр сцены.
        /// </summary>
        public Scene()
        {
            // Создайте контент-менеджер для сцены
            Content = new ContentManager(Core.Content.ServiceProvider);

            // Установите корневой каталог для контента таким же, как и корневой каталог для игрового контента.
            Content.RootDirectory = Core.Content.RootDirectory;
        }

        // Финализатор, вызываемый сборщиком мусора при очистке объекта.
        ~Scene() => Dispose(false);

        /// <summary>
        /// Initializes the scene.
        /// </summary>
        /// <remarks>
        /// When overriding this in a derived class, ensure that base.Initialize()
        /// still called as this is when LoadContent is called.
        /// </remarks>
        public virtual void Initialize()
        {
            LoadContent();
        }

        /// <summary>
        /// Override to provide logic to load content for the scene.
        /// </summary>
        public virtual void LoadContent() { }

        /// <summary>
        /// Unloads scene-specific content.
        /// </summary>
        public virtual void UnloadContent()
        {
            Content.Unload();
        }

        /// <summary>
        /// Updates this scene.
        /// </summary>
        /// <param name="gameTime">A snapshot of the timing values for the current frame.</param>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// Draws this scene.
        /// </summary>
        /// <param name="gameTime">A snapshot of the timing values for the current frame.</param>
        public virtual void Draw(GameTime gameTime) { }

        /// <summary>
        /// Disposes of this scene.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of this scene.
        /// </summary>
        /// <param name="disposing">'
        /// Indicates whether managed resources should be disposed.  This value is only true when called from the main
        /// Dispose method.  When called from the finalizer, this will be false.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                UnloadContent();
                Content.Dispose();
            }
        }
    }
}
