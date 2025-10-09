using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJumpLibrary.Graphics
{
    public class Animation
    {
        /// <summary>
        /// Области текстуры, из которых состоят кадры этой анимации.  Порядок расположения областей в коллекции
        /// - это порядок, в котором должны отображаться кадры.
        /// </summary>
        public List<TextureRegion> Frames { get; set; }

        /// <summary>
        /// The amount of time to delay between each frame before moving to the next frame for this animation.
        /// </summary>
        public TimeSpan Delay { get; set; }

        /// <summary>
        /// Creates a new animation.
        /// </summary>
        public Animation()
        {
            Frames = new List<TextureRegion>();
            Delay = TimeSpan.FromMilliseconds(100);
        }

        /// <summary>
        /// Creates a new animation with the specified frames and delay.
        /// </summary>
        /// <param name="frames">An ordered collection of the frames for this animation.</param>
        /// <param name="delay">The amount of time to delay between each frame of this animation.</param>
        public Animation(List<TextureRegion> frames, TimeSpan delay)
        {
            Frames = frames;
            Delay = delay;
        }
    }
}
