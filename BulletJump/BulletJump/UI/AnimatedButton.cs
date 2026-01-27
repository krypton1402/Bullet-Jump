using BulletJumpLibrary.Graphics;
using BulletJumpLibrary.Graphics.Animations;
using Gum.DataTypes.Variables;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals;
using Gum.Graphics.Animation;
using Gum.Managers;
using Microsoft.Xna.Framework.Input;
using MonoGameGum.GueDeriving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJump.UI
{
    internal class AnimatedButton : Button
    {
        public AnimatedButton(TextureAtlas atlas)
        {
            ButtonVisual buttonVisual = (ButtonVisual)Visual;

            buttonVisual.Height = 14f;
            buttonVisual.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            buttonVisual.Width = 21f;
            buttonVisual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToChildren;

            NineSliceRuntime background = buttonVisual.Background;
            background.Texture = atlas.Texture;
            background.TextureAddress = TextureAddress.Custom;
            background.Color = Microsoft.Xna.Framework.Color.White;

            TextRuntime textInstance = buttonVisual.TextInstance;
            textInstance.Text = "Новая игра";
            textInstance.Blue = 130;
            textInstance.Green = 86;
            textInstance.Red = 70;
            textInstance.UseCustomFont = true;
            textInstance.CustomFontFile = @"fonts/drukwidecyr-bold.ttf";
            textInstance.FontScale = 0.25f;
            textInstance.Anchor(Gum.Wireframe.Anchor.Center);
            textInstance.Width = 0;
            textInstance.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToChildren;

            TextureRegion unfocusedTextureRegion = atlas.GetRegion("unfocused-button");
            AnimationChain unfocusedAnimation = new AnimationChain();
            unfocusedAnimation.Name = nameof(unfocusedAnimation);
            AnimationFrame unfocusedFrame = new AnimationFrame
            {
                TopCoordinate = unfocusedTextureRegion.TopTextureCoordinate,
                BottomCoordinate = unfocusedTextureRegion.BottomTextureCoordinate,
                LeftCoordinate = unfocusedTextureRegion.LeftTextureCoordinate,
                RightCoordinate = unfocusedTextureRegion.RightTextureCoordinate,
                FrameLength = 0.3f,
                Texture = unfocusedTextureRegion.Texture,
            };
            unfocusedAnimation.Add(unfocusedFrame);

            Animation focusedAtlasAnimation = atlas.GetAnimation("focused-button-animation");

            AnimationChain focusedAnimation = new AnimationChain();
            focusedAnimation.Name = nameof(focusedAnimation);
            foreach (TextureRegion region in focusedAtlasAnimation.Frames)
            {
                AnimationFrame frame = new AnimationFrame
                {
                    TopCoordinate = region.TopTextureCoordinate,
                    BottomCoordinate = region.BottomTextureCoordinate,
                    LeftCoordinate = region.LeftTextureCoordinate,
                    RightCoordinate = region.RightTextureCoordinate,
                    FrameLength = (float)focusedAtlasAnimation.Delay.TotalSeconds,
                    Texture = region.Texture
                };

                focusedAnimation.Add(frame);
            }

            background.AnimationChains = new AnimationChainList
            {
                unfocusedAnimation,
                focusedAnimation,
            };

            buttonVisual.ButtonCategory.ResetAllStates();

            StateSave enabledState = buttonVisual.States.Enabled;
            enabledState.Apply = () =>
            {

                background.CurrentChainName = unfocusedAnimation.Name;
            };

            StateSave focusedState = buttonVisual.States.Focused;
            focusedState.Apply = () =>
            {
                background.CurrentChainName = focusedAnimation.Name;
                background.Animate = true;
            };

            // Создать выделенное+сфокусированное состояние (для наведения курсора мыши во время фокусировки)
            StateSave highlightedFocused = buttonVisual.States.HighlightedFocused;
            highlightedFocused.Apply = focusedState.Apply;

            // Создайте выделенное состояние (при наведении курсора мыши)
            // путем клонирования включенного состояния, поскольку они выглядят одинаково
            StateSave highlighted = buttonVisual.States.Highlighted;
            highlighted.Apply = enabledState.Apply;

            // Add event handlers for keyboard input.
            KeyDown += HandleKeyDown;

            // Добавить обработчик событий для фокусировки при наведении курсора мыши.
            buttonVisual.RollOn += HandleRollOn;
        }

        /// <summary>
        /// Обрабатывает ввод с клавиатуры для навигации между кнопками с помощью клавиш влево/вправо.
        /// </summary>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Left)
            {
                HandleTab(TabDirection.Up, loop: true);
            }
            if (e.Key == Keys.Right)
            {
                HandleTab(TabDirection.Down, loop: true);
            }
        }

        private void HandleRollOn(object sender, EventArgs e)
        {
            IsFocused = true;
        }
    }
}
