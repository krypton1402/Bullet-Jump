using BulletJumpLibrary.Graphics.Animations;
using Gum.Graphics.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJumpLibrary.Graphics
{
    public static class TextureAtlasExtensions
    {
        public static AnimationChainList CreateAnimationChainList(this TextureAtlas atlas,
            Dictionary<string, string> animationMappings)
        {
            var chainList = new AnimationChainList();

            foreach (var mapping in animationMappings)
            {
                var animationName = mapping.Key;
                var chainName = mapping.Value;

                var animation = atlas.GetAnimation(animationName);
                if (animation != null)
                {
                    var chain = CreateAnimationChainFromAtlasAnimation(animation, chainName);
                    chainList.Add(chain);
                }
            }

            return chainList;
        }

        public static AnimationChain CreateAnimationChainFromAtlasAnimation(Animation atlasAnimation, string chainName)
        {
            var chain = new AnimationChain { Name = chainName };

            foreach (var region in atlasAnimation.Frames)
            {
                var frame = new AnimationFrame
                {
                    TopCoordinate = region.TopTextureCoordinate,
                    BottomCoordinate = region.BottomTextureCoordinate,
                    LeftCoordinate = region.LeftTextureCoordinate,
                    RightCoordinate = region.RightTextureCoordinate,
                    FrameLength = (float)atlasAnimation.Delay.TotalSeconds,
                    Texture = region.Texture
                };
                chain.Add(frame);
            }

            return chain;
        }

        public static AnimationChain CreateSingleFrameChain(this TextureAtlas atlas,
            string regionName, string chainName, float frameLength = 0.1f)
        {
            var region = atlas.GetRegion(regionName);
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
