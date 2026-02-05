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

namespace BulletJump.UI
{
    internal class AnimatedButton : Button
    {
        public AnimatedButton(TextureAtlas atlas, float scale)
        {
            ButtonVisual buttonVisual = (ButtonVisual)Visual;

            buttonVisual.Height = 87f;
            buttonVisual.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            buttonVisual.Width = 516f;
            buttonVisual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToChildren;


            NineSliceRuntime background = buttonVisual.Background;
            background.Texture = atlas.Texture;
            background.TextureAddress = TextureAddress.Custom;
            background.Color = Microsoft.Xna.Framework.Color.White;

            TextRuntime textInstance = buttonVisual.TextInstance;
            textInstance.Text = "New Game";
            textInstance.Blue = 255;
            textInstance.Green = 255;
            textInstance.Red = 70;
            textInstance.UseCustomFont = true;
            // textInstance.Font = "Arial";
            textInstance.CustomFontFile = @"fonts/drukwidecyr-bold.fnt";
            textInstance.FontSize = 64;
            textInstance.Anchor(Gum.Wireframe.Anchor.Center);
            textInstance.Width = 0;
            textInstance.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToChildren;

            TextureRegion unfocusedTextureRegion = atlas.GetRegion("unfocused-button");
            AnimationChain unfocusedAnimation = atlas.CreateSingleFrameChain(
                "unfocused-button",
                "Unfocused",
                0.3f);

            AnimationChain focusedAtlasAnimationChain = atlas.GetAnimationChain("focused-button-animation");

            AnimationChain focusedAnimation = new AnimationChain();
            focusedAnimation.Name = "Focused";

            if (focusedAtlasAnimationChain != null)
            {
                // Копируем каждый кадр из исходной анимации
                foreach (AnimationFrame frameInAtlas in focusedAtlasAnimationChain)
                {
                    AnimationFrame frame = new AnimationFrame
                    {
                        TopCoordinate = frameInAtlas.TopCoordinate,
                        BottomCoordinate = frameInAtlas.BottomCoordinate,
                        LeftCoordinate = frameInAtlas.LeftCoordinate,
                        RightCoordinate = frameInAtlas.RightCoordinate,
                        FrameLength = frameInAtlas.FrameLength,
                        Texture = frameInAtlas.Texture
                    };
                    focusedAnimation.Add(frame);
                }
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
