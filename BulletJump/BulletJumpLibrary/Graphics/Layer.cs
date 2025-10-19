using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJumpLibrary.Graphics
{
    public class Layer
    {
        private readonly int[] _tiles;

        public string LayerName { get; }
        public int Rows { get; }
        public int Columns { get; }
        public int Count { get; }
        public bool IsVisible { get; set; } = true; // Добавляем setter

        // Исправляем порядок параметров: сначала columns, потом rows
        public Layer(string name, int columns, int rows)
        {
            LayerName = name;
            Columns = columns;
            Rows = rows;
            Count = columns * rows;
            _tiles = new int[Count];
        }

        public void SetTile(int index, int tilesetID)
        {
            _tiles[index] = tilesetID;
        }

        public void SetTile(int column, int row, int tilesetID)
        {
            int index = row * Columns + column;
            SetTile(index, tilesetID);
        }

        public int GetTileID(int index)
        {
            return _tiles[index];
        }

        public int GetTileID(int column, int row)
        {
            int index = row * Columns + column;
            return GetTileID(index);
        }

        public bool IsTileEmpty(int column, int row)
        {
            return GetTileID(column, row) == 0;
        }
    }
}
