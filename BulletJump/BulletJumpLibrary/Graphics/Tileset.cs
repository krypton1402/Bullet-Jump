using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJumpLibrary.Graphics
{
    public class Tileset
    {
        private readonly TextureRegion[] _tiles;

        /// <summary>
        /// Gets the width, in pixels, of each tile in this tileset.
        /// </summary>
        public int TileWidth { get; }

        /// <summary>
        /// Gets the height, in pixels, of each tile in this tileset.
        /// </summary>
        public int TileHeight { get; }

        /// <summary>
        /// Gets the total number of columns in this tileset.
        /// </summary>
        public int Columns { get; }

        /// <summary>
        /// Gets the total number of rows in this tileset.
        /// </summary>
        public int Rows { get; }

        /// <summary>
        /// Gets the total number of tiles in this tileset.
        /// </summary>
        public int Count { get; }

        public Tileset(TextureRegion textureRegion, int tileWidth, int tileHeight)
        {
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            Columns = textureRegion.Width / tileHeight;
            Rows = textureRegion.Height / tileHeight;
            Count = Columns * Rows;

            _tiles = new TextureRegion[Count];

            for (int i = 0; i < Count; i++)
            {
                int x = i % Columns * tileWidth;
                int y = i / Columns * tileHeight;
                _tiles[i] = new TextureRegion(textureRegion.Texture, textureRegion.SourceRectangle.X + x, textureRegion.SourceRectangle.Y + y, tileWidth, tileHeight);
            }
        }

        /// <summary>
        /// Возвращает область текстуры для плитки из этого набора листов с заданным индексом.
        /// </summary>
        /// <param name="index">Индекс области текстуры в этом наборе плиток.</param>
        /// <returns>Область текстуры для плитки формирует этот набор листов с заданным индексом</returns>
        public TextureRegion GetTile(int index) => _tiles[index];

        /// <summary>
        /// Получает область текстуры для тайла из этого набора тайлов в указанном месте.
        /// </summary>
        /// <param name="column">Столбец в этом наборе плиток с текстурой.</param>
        /// <param name="row">Строка в этом наборе плиток в области текстуры.</param>
        /// <returns>Область текстуры для плитки из этого набора в указанном месте.</returns>
        public TextureRegion GetTile(int column, int row)
        {
            int index = row * Columns + column;
            return GetTile(index);
        }
    }
}
