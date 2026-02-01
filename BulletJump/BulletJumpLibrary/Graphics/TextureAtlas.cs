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
using BulletJumpLibrary.Graphics.Animations;
using Gum.Graphics.Animation;

namespace BulletJumpLibrary.Graphics
{
    public class TextureAtlas
    {
        // Stores animations added to this atlas.
        private Dictionary<string, AnimationChain> _animationChains;

        private Dictionary<string, TextureRegion> _regions;

        /// <summary>
        /// Gets or Sets the source texture represented by this texture atlas.
        /// </summary>
        public Texture2D Texture { get; set; }

        public TextureAtlas()
        {
            _regions = new Dictionary<string, TextureRegion>();
            _animationChains = new Dictionary<string, AnimationChain>();
        }

        /// <summary>
        /// Создает новый экземпляр текстурного атласа, используя заданную текстуру.
        /// </summary>
        /// <param name="texture">The source texture represented by the texture atlas.</param>
        public TextureAtlas(Texture2D texture)
        {
            Texture = texture;
            _regions = new Dictionary<string, TextureRegion>();
            _animationChains = new Dictionary<string, AnimationChain>();
        }

        /// <summary>
        /// Creates a new region and adds it to this texture atlas.
        /// </summary>
        /// <param name="name">Название, которое будет присвоено области текстуры</param>
        /// <param name="x">Положение границы области по координате x в верхнем левом углу относительно верхнего левого угла границы исходной текстуры.</param>
        /// <param name="y">Положение границы области по координате y в верхнем левом углу относительно верхнего левого угла границы исходной текстуры.</param>
        /// <param name="width">The width, in pixels, of the region.</param>
        /// <param name="height">The height, in pixels, of the region.</param>
        public void AddRegion(string name, int x, int y, int width, int height)
        {
            TextureRegion region = new TextureRegion(Texture, x, y, width, height);
            _regions.Add(name, region);
        }

        /// <summary>
        /// Получает область из этого текстурного атласа с указанным названием.
        /// </summary>
        /// <param name="name">The name of the region to retrieve.</param>
        /// <returns>The TextureRegion with the specified name.</returns>
        public TextureRegion GetRegion(string name)
        {
            return _regions[name];
        }

        /// <summary>
        /// Removes the region from this texture atlas with the specified name.
        /// </summary>
        /// <param name="name">The name of the region to remove.</param>
        /// <returns></returns>
        public bool RemoveRegion(string name)
        {
            return _regions.Remove(name);
        }

        /// <summary>
        /// Removes all regions from this texture atlas.
        /// </summary>
        public void Clear()
        {
            _regions.Clear();
        }

        /// <summary>
        /// Creates a new texture atlas based a texture atlas xml configuration file.
        /// </summary>
        /// <param name="content">The content manager used to load the texture for the atlas.</param>
        /// <param name="fileName">The path to the xml file, relative to the content root directory.</param>
        /// <returns>The texture atlas created by this method.</returns>
        public static TextureAtlas FromFile(ContentManager content, string fileName)
        {
            TextureAtlas atlas = new TextureAtlas();

            string filePath = Path.Combine(content.RootDirectory, fileName);

            using (Stream stream = TitleContainer.OpenStream(filePath))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    XDocument doc = XDocument.Load(reader);
                    XElement root = doc.Root;

                    // Элемент <Texture> содержит путь к содержимому, который должен быть загружен Texture2D.
                    // Итак, мы получим это значение, а затем используем content manager для загрузки текстуры.
                    string texturePath = root.Element("Texture").Value;
                    atlas.Texture = content.Load<Texture2D>(texturePath);

                    // Элемент <Regions> содержит отдельные элементы <Region>, каждый из которых описывает
                    // различные текстурные области в атласе.  
                    //
                    // Example:
                    // <Regions>
                    //      <Region name="spriteOne" x="0" y="0" width="32" height="32" />
                    //      <Region name="spriteTwo" x="32" y="0" width="32" height="32" />
                    // </Regions>
                    //
                    // Итак, мы извлекаем все элементы <Region>, затем перебираем каждый из них 
                    // и создаем из него новый экземпляр TextureRegion и добавляем его в этот атлас.
                    var regions = root.Element("Regions")?.Elements("Region");

                    if (regions != null)
                    {
                        foreach (var region in regions)
                        {
                            string name = region.Attribute("name")?.Value;
                            int x = int.Parse(region.Attribute("x")?.Value ?? "0");
                            int y = int.Parse(region.Attribute("y")?.Value ?? "0");
                            int width = int.Parse(region.Attribute("width")?.Value ?? "0");
                            int height = int.Parse(region.Attribute("height")?.Value ?? "0");

                            if (!string.IsNullOrEmpty(name))
                            {
                                atlas.AddRegion(name, x, y, width, height);
                            }
                        }
                    }

                    // The <Animations> element contains individual <Animation> elements, each one describing
                    // a different animation within the atlas.
                    //
                    // Example:
                    // <Animations>
                    //      <Animation name="animation" delay="100">
                    //          <Frame region="spriteOne" />
                    //          <Frame region="spriteTwo" />
                    //      </Animation>
                    // </Animations>
                    //
                    // So we retrieve all of the <Animation> elements then loop through each one
                    // and generate a new Animation instance from it and add it to this atlas.
                    var animationElements = root.Element("Animations").Elements("Animation");

                    if (animationElements != null)
                    {
                        foreach (var animationElement in animationElements)
                        {
                            string name = animationElement.Attribute("name")?.Value;
                            float delayInMilliseconds = float.Parse(animationElement.Attribute("delay")?.Value ?? "0");

                            float frameLength = delayInMilliseconds / 1000.0f; // Конвертируем в секунды

                            var chain = new AnimationChain { Name = name };

                            var frameElements = animationElement.Elements("Frame");

                            if (frameElements != null)
                            {
                                foreach (var frameElement in frameElements)
                                {
                                    string regionName = frameElement.Attribute("region").Value;
                                    TextureRegion region = atlas.GetRegion(regionName);
                                    var frame = new AnimationFrame
                                    {
                                        TopCoordinate = region.TopTextureCoordinate,
                                        BottomCoordinate = region.BottomTextureCoordinate,
                                        LeftCoordinate = region.LeftTextureCoordinate,
                                        RightCoordinate = region.RightTextureCoordinate,
                                        FrameLength = frameLength,
                                        Texture = region.Texture
                                    };
                                    chain.Add(frame);
                                }
                            }

                            atlas.AddAnimationChain(name, chain);
                        }
                    }

                    return atlas;
                }
            }
        }

        /// <summary>
        /// Creates a new sprite using the region from this texture atlas with the specified name.
        /// </summary>
        /// <param name="regionName">The name of the region to create the sprite with.</param>
        /// <returns>A new Sprite using the texture region with the specified name.</returns>
        public Sprite CreateSprite(string regionName)
        {
            TextureRegion region = GetRegion(regionName);
            return new Sprite(region);
        }

        public void AddAnimationChain(string name, AnimationChain chain)
        {
            _animationChains.Add(name, chain);
        }

        public AnimationChain GetAnimationChain(string name)
        {
            return _animationChains[name];
        }

        /// <summary>
        /// Creates an AnimationChainList from animation name mappings
        /// </summary>
        public AnimationChainList CreateAnimationChainList(Dictionary<string, string> animationMappings)
        {
            var chainList = new AnimationChainList();

            foreach (var mapping in animationMappings)
            {
                var animationName = mapping.Key;
                var chainName = mapping.Value;

                var chain = GetAnimationChain(animationName);
                if (chain != null)
                {
                    // Создаем копию с новым именем
                    var newChain = new AnimationChain { Name = chainName };
                    foreach (var frame in chain)
                    {
                        newChain.Add(frame);
                    }
                    chainList.Add(newChain);
                }
            }

            return chainList;
        }

        /// <summary>
        /// Creates a single-frame AnimationChain from a region
        /// </summary>
        public AnimationChain CreateSingleFrameChain(string regionName, string chainName, float frameLength = 0.1f)
        {
            var region = GetRegion(regionName);
            if (region == null)
                return null;

            var chain = new AnimationChain { Name = chainName };
            var frame = new AnimationFrame
            {
                TopCoordinate = region.TopTextureCoordinate,
                BottomCoordinate = region.BottomTextureCoordinate,
                LeftCoordinate = region.LeftTextureCoordinate,
                RightCoordinate = region.RightTextureCoordinate,
                FrameLength = frameLength,
                Texture = region.Texture
            };
            chain.Add(frame);

            return chain;
        }
    }
}
