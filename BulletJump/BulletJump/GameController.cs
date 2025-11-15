using BulletJumpLibrary.Input;
using BulletJumpLibrary;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace BulletJump
{
    public static class GameController
    {
        private static KeyboardInfo s_keyboard => Core.Input.Keyboard;
        private static GamePadInfo s_gamePad => Core.Input.GamePads[(int)PlayerIndex.One];

        public static bool MoveUp()
        {
            return s_keyboard.IsKeyDown(Keys.Up) ||
                   s_keyboard.IsKeyDown(Keys.W) ||
                   s_gamePad.IsButtonDown(Buttons.DPadUp) ||
                   s_gamePad.IsButtonDown(Buttons.LeftThumbstickUp);
        }

        public static bool MoveDown()
        {
            return s_keyboard.IsKeyDown(Keys.Down) ||
                   s_keyboard.IsKeyDown(Keys.S) ||
                   s_gamePad.IsButtonDown(Buttons.DPadDown) ||
                   s_gamePad.IsButtonDown(Buttons.LeftThumbstickDown);
        }

        public static bool MoveLeft()
        {
            return s_keyboard.IsKeyDown(Keys.Left) ||
                   s_keyboard.IsKeyDown(Keys.A) ||
                   s_gamePad.IsButtonDown(Buttons.DPadLeft) ||
                   s_gamePad.IsButtonDown(Buttons.LeftThumbstickLeft);
        }
        public static bool MoveRight()
        {
            return s_keyboard.IsKeyDown(Keys.Right) ||
                   s_keyboard.IsKeyDown(Keys.D) ||
                   s_gamePad.IsButtonDown(Buttons.DPadRight) ||
                   s_gamePad.IsButtonDown(Buttons.LeftThumbstickRight);
        }

        public static bool Pause()
        {
            return s_keyboard.WasKeyJustPressed(Keys.Escape) ||
                   s_gamePad.WasButtonJustPressed(Buttons.Start);
        }

        public static bool Action()
        {
            return s_keyboard.WasKeyJustPressed(Keys.Enter) ||
                   s_gamePad.WasButtonJustPressed(Buttons.A);
        }

        public static bool Shot()
        {
            return s_keyboard.IsKeyDown(Keys.Space) ||
                   s_gamePad.IsButtonDown(Buttons.A);
        }
    }
}
