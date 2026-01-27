using Gum.Graphics.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJumpLibrary.Graphics.Interfaces
{
    public interface IAnimatedSprite
    {
        AnimationChainList AnimationChains { get; set; }
        string CurrentChainName { get; set; }
        bool Animate { get; set; }
        Vector2 Position { get; set; }
        Vector2 Origin { get; set; }
        Vector2 Scale { get; set; }
        SpriteEffects Effects { get; set; }
        float Rotation { get; set; }
        Color Color { get; set; }

        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
        void ResetAnimation();
    }
}
