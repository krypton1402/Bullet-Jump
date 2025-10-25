using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJumpLibrary.Graphics
{
    public class Camera
    {
        private Vector2 _position;
        private float _zoom;
        private float _rotation;
        private Viewport _viewport;

        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }

        public float Zoom
        {
            get => _zoom;
            set => _zoom = MathHelper.Clamp(value, 0.1f, 10f);
        }

        public float Rotation
        {
            get => _rotation;
            set => _rotation = value;
        }

        public Vector2 Target { get; set; }

        public float Smoothness { get; set; } = 0.1f;

        public Rectangle Bounds { get; set; }


        public Camera(Viewport viewport)
        {
            _viewport = viewport;
            _zoom = 0.7f;
            _rotation = 0.0f;
            _position = Vector2.Zero;
        }

        public Matrix GetTransformMatrix()
        {
            return Matrix.CreateTranslation(-_position.X, -_position.Y, 0) *
                   Matrix.CreateRotationZ(_rotation) *
                   Matrix.CreateScale(_zoom) *
                   Matrix.CreateTranslation(_viewport.Width * 0.5f, _viewport.Height * 0.5f, 0);
        }

        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, GetTransformMatrix());
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(GetTransformMatrix()));
        }

        public void Update(GameTime gameTime)
        {
            if (Target != Vector2.Zero)
            {
                // Просто плавно следуем за целью без ограничений
                _position = Vector2.Lerp(_position, Target, Smoothness);
            }
        }


        public void SetBoundsFromTilemap(Tilemap tilemap)
        {
            Bounds = new Rectangle(0, 0,
                (int)(tilemap.Columns * tilemap.TileWidth),
                (int)(tilemap.Rows * tilemap.TileHeight)
                );
        }

        public bool IsInView(Rectangle bounds)
        {
            Rectangle cameraView = new Rectangle(
                (int)(_position.X - _viewport.Width / (2 * _zoom)),
                (int)(_position.Y - _viewport.Height / (2 * _zoom)),
                (int)(_viewport.Width / _zoom),
                (int)(_viewport.Height / _zoom)
            );

            return cameraView.Intersects(bounds);
        }
    }
}
