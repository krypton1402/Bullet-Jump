using BulletJumpLibrary.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJump.Scenes
{
    public class TitleScene : Scene
    {
        private const string BULLET_TEXT = "Bullet";
        private const string JUMP_TEXT = "Jump";
        private const string PRESS_ENTER_TEXT = "Press Enter To Start";

        // The font to use to render normal text.
        private SpriteFont _font;

        // The font used to render the title text.
        private SpriteFont _font5x;

        // The position to draw the dungeon text at.
        private Vector2 _bulletTextPos;

        // The origin to set for the dungeon text.
        private Vector2 _bulletTextOrigin;

        // The position to draw the slime text at.
        private Vector2 _jumpTextPos;

        // The origin to set for the slime text.
        private Vector2 _jumpTextOrigin;

        // The position to draw the press enter text at.
        private Vector2 _pressEnterPos;

        // The origin to set for the press enter text when drawing it.
        private Vector2 _pressEnterOrigin;

        // Фоновый паттерн
        private Texture2D _backgrounPattern;

        // Целевой прямоугольник, который будет заполнен фоновым рисунком.
        private Rectangle _backgroundDestination;
    }
}
