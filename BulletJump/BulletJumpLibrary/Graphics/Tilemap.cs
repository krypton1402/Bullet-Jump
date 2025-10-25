using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace BulletJumpLibrary.Graphics
{
    public class Tilemap
    {
        private readonly Tileset _tileset;
        private readonly Dictionary<string, Layer> _layers;

        /// <summary>
        /// Gets the total number of rows in this tilemap.
        /// </summary>
        public int Rows { get; }

        /// <summary>
        /// Gets the total number of columns in this tilemap.
        /// </summary>
        public int Columns { get; }

        /// <summary>
        /// Gets the total number of tiles in this tilemap.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Возвращает или устанавливает масштабный коэффициент для рисования каждой плитки.
        /// </summary>
        public Vector2 Scale { get; set; }

        /// <summary>
        /// Gets the width, in pixels, each tile is drawn at.
        /// </summary>
        public float TileWidth => _tileset.TileWidth * Scale.X;

        /// <summary>
        /// Gets the height, in pixels, each tile is drawn at.
        /// </summary>
        public float TileHeight => _tileset.TileHeight * Scale.Y;

        public Dictionary<string, Layer> Layers => _layers;

        /// <summary>
        /// Creates a new tilemap.
        /// </summary>
        /// <param name="tileset">The tileset used by this tilemap.</param>
        /// <param name="columns">The total number of columns in this tilemap.</param>
        /// <param name="rows">The total number of rows in this tilemap.</param>
        public Tilemap(Tileset tileset, int columns, int rows)
        {
            _tileset = tileset;
            _layers = new Dictionary<string, Layer>();
            Rows = rows;
            Columns = columns;
            Scale = Vector2.One;
        }

        public void AddLayer(Layer layer)
        {
            if (layer.Columns != Columns || layer.Rows != Rows)
            {
                throw new ArgumentException("Разрешение слоя должно совпадать с разрешением tilemap");
            }
            _layers.Add(layer.LayerName, layer);
        }

        public Layer GetLayer(string name)
        {
            return _layers[name];
        }

        public bool RemoveLayer(string name)
        {
            return _layers.Remove(name);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, null);
        }

        public void Draw(SpriteBatch spriteBatch, IEnumerable<string> layerNames)
        {
            var layersToDraw = layerNames != null
                ? _layers.Where(l => layerNames.Contains(l.Key) && l.Value.IsVisible)
                : _layers.Where(l => l.Value.IsVisible);

            foreach (var layer in layersToDraw)
            {
                DrawLayer(spriteBatch, layer.Value);
            }
        }

        public void DrawLayer(SpriteBatch spriteBatch, Layer layer)
        {
            if (!layer.IsVisible) return;

            for (int i = 0; i < layer.Count; i++)
            {
                int tilesetIndex = layer.GetTileID(i);
                if (tilesetIndex == 0) continue; // Skip empty tiles

                TextureRegion tile = _tileset.GetTile(tilesetIndex);

                int x = i % Columns;
                int y = i / Columns;

                Vector2 position = new Vector2(x * TileWidth, y * TileHeight);
                tile.Draw(spriteBatch, position, Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 1.0f);
            }
        }

        /// <summary>
        /// Gets the texture region of the tile from this tilemap at the specified index.
        /// </summary>
        /// <param name="index">The index of the tile in this tilemap.</param>
        /// <returns>The texture region of the tile from this tilemap at the specified index.</returns>
        public TextureRegion GetTile(string layerName, int column, int row)
        {
            if (!_layers.ContainsKey(layerName)) return null;

            int tilesetIndex = _layers[layerName].GetTileID(column, row);
            return tilesetIndex == 0 ? null : _tileset.GetTile(tilesetIndex);
        }

        public bool IsTileEmpty(string layerName, int column, int row)
        {
            return !_layers.ContainsKey(layerName) || _layers[layerName].IsTileEmpty(column, row);
        }

        /// <summary>
        /// Creates a new tilemap based on a tilemap xml configuration file.
        /// </summary>
        /// <param name="content">The content manager used to load the texture for the tileset.</param>
        /// <param name="filename">The path to the xml file, relative to the content root directory.</param>
        /// <returns>The tilemap created by this method.</returns>
        public static Tilemap FromFile(ContentManager content, string filename)
        {
            string filePath = Path.Combine(content.RootDirectory, filename);

            using (Stream stream = TitleContainer.OpenStream(filePath))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    XDocument doc = XDocument.Load(reader);
                    XElement root = doc.Root;

                    // Load tileset
                    XElement tilesetElement = root.Element("Tileset");

                    string regionAttribute = tilesetElement.Attribute("region").Value;
                    string[] split = regionAttribute.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    int x = int.Parse(split[0]);
                    int y = int.Parse(split[1]);
                    int width = int.Parse(split[2]);
                    int height = int.Parse(split[3]);

                    int tileWidth = int.Parse(tilesetElement.Attribute("tileWidth").Value);
                    int tileHeight = int.Parse(tilesetElement.Attribute("tileHeight").Value);
                    string contentPath = tilesetElement.Value;

                    Texture2D texture = content.Load<Texture2D>(contentPath);
                    TextureRegion textureRegion = new TextureRegion(texture, x, y, width, height);
                    Tileset tileset = new Tileset(textureRegion, tileWidth, tileHeight);

                    // Determine tilemap dimensions from the first layer
                    XElement layersElement = root.Element("Layers");
                    XElement firstLayer = layersElement.Elements("Layer").First();
                    XElement firstTilesElement = firstLayer.Element("Tiles");

                    string[] firstLayerRows = firstTilesElement.Value.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    int columnCount = firstLayerRows[0].Split(" ", StringSplitOptions.RemoveEmptyEntries).Length;
                    int rowCount = firstLayerRows.Length;

                    // Create the tilemap
                    Tilemap tilemap = new Tilemap(tileset, columnCount, rowCount);

                    // Process each layer
                    foreach (XElement layerElement in layersElement.Elements("Layer"))
                    {
                        string layerName = layerElement.Attribute("name").Value;
                        XElement tilesElement = layerElement.Element("Tiles");

                        string[] layerRows = tilesElement.Value.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);

                        // Создаем слой с правильными размерами - columns, rows
                        Layer layer = new Layer(layerName, columnCount, rowCount);

                        // Process each row of the current layer
                        for (int row = 0; row < rowCount; row++)
                        {
                            string[] columns = layerRows[row].Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);

                            for (int column = 0; column < columnCount; column++)
                            {
                                int tilesetIndex = int.Parse(columns[column]);
                                layer.SetTile(column, row, tilesetIndex);
                            }
                        }

                        tilemap.AddLayer(layer);
                    }

                    return tilemap;
                }

            }
        }
    }
}
