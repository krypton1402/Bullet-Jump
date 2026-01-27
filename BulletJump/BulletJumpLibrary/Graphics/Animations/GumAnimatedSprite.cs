using BulletJumpLibrary.Graphics.Interfaces;
using Gum.Graphics.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BulletJumpLibrary.Graphics.Animations
{
    public class GumAnimatedSprite : IAnimatedSprite
    {
        private float _elapsedTime;
        private int _currentFrameIndex;
        private AnimationChain _currentChain;
        private string _previousChainName = string.Empty;

        public AnimationChainList AnimationChains { get; set; }
        public string CurrentChainName { get; set; }
        public bool Animate { get; set; } = true;
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }
        public Vector2 Scale { get; set; } = Vector2.One;
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;
        public float Rotation { get; set; }
        public Color Color { get; set; } = Color.White;

        public GumAnimatedSprite()
        {
            AnimationChains = new AnimationChainList();
        }

        public GumAnimatedSprite(AnimationChainList animationChains)
        {
            AnimationChains = animationChains ?? new AnimationChainList();
        }

        public void Update(GameTime gameTime)
        {
            // Если анимация отключена, но нужно прорисовать статичный кадр
            if (string.IsNullOrEmpty(CurrentChainName))
                return;

            // Проверяем, изменилась ли анимация
            bool chainChanged = false;
            if (_previousChainName != CurrentChainName || _currentChain == null)
            {
                _currentChain = GetAnimationChain(CurrentChainName);
                _previousChainName = CurrentChainName;
                chainChanged = true;
            }

            if (_currentChain == null || _currentChain.Count == 0)
            {
                // Пробуем использовать первую доступную анимацию
                if (AnimationChains != null && AnimationChains.Count > 0)
                {
                    _currentChain = AnimationChains[0];
                    CurrentChainName = _currentChain.Name;
                    _previousChainName = CurrentChainName;
                    chainChanged = true;
                }
                else
                {
                    return;
                }
            }

            // При смене анимации сбрасываем таймер
            if (chainChanged)
            {
                _elapsedTime = 0;
                _currentFrameIndex = 0;
            }

            // Если анимация включена - обновляем кадр
            if (Animate)
            {
                _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Проверяем индекс кадра
                if (_currentFrameIndex < 0 || _currentFrameIndex >= _currentChain.Count)
                {
                    _currentFrameIndex = 0;
                }

                var currentFrame = _currentChain[_currentFrameIndex];
                float frameLength = Math.Max(currentFrame.FrameLength, 0.001f);

                // Переход к следующему кадру
                while (_elapsedTime >= frameLength && frameLength > 0)
                {
                    _elapsedTime -= frameLength;
                    _currentFrameIndex = (_currentFrameIndex + 1) % _currentChain.Count;
                    currentFrame = _currentChain[_currentFrameIndex];
                    frameLength = Math.Max(currentFrame.FrameLength, 0.001f);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (string.IsNullOrEmpty(CurrentChainName) || _currentChain == null || _currentChain.Count == 0)
                return;

            // Проверяем индекс кадра
            if (_currentFrameIndex < 0 || _currentFrameIndex >= _currentChain.Count)
            {
                _currentFrameIndex = 0;
            }

            var frame = _currentChain[_currentFrameIndex];
            if (frame.Texture == null) return;

            try
            {
                // Вычисляем прямоугольник источника
                int left = (int)(frame.LeftCoordinate * frame.Texture.Width);
                int top = (int)(frame.TopCoordinate * frame.Texture.Height);
                int width = (int)((frame.RightCoordinate - frame.LeftCoordinate) * frame.Texture.Width);
                int height = (int)((frame.BottomCoordinate - frame.TopCoordinate) * frame.Texture.Height);

                // Проверяем валидность размеров
                if (width <= 0 || height <= 0) return;

                var sourceRectangle = new Rectangle(left, top, width, height);

                // Отрисовываем кадр
                spriteBatch.Draw(
                    frame.Texture,
                    Position,
                    sourceRectangle,
                    Color,
                    Rotation,
                    Origin,
                    Scale,
                    Effects,
                    0f
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отрисовки анимации: {ex.Message}");
            }
        }

        public void ResetAnimation()
        {
            _elapsedTime = 0;
            _currentFrameIndex = 0;
        }

        public void Play(string chainName)
        {
            if (CurrentChainName != chainName || !Animate)
            {
                CurrentChainName = chainName;
                Animate = true;
                _previousChainName = string.Empty; // Форсируем смену анимации
            }
        }

        public void Stop()
        {
            Animate = false;
            ResetAnimation();
        }

        public void Pause()
        {
            Animate = false;
        }

        public void Resume()
        {
            Animate = true;
        }

        public bool IsComplete()
        {
            if (_currentChain == null || _currentChain.Count == 0)
                return false;

            return !Animate || (_currentFrameIndex == _currentChain.Count - 1 &&
                   _elapsedTime >= _currentChain[_currentFrameIndex].FrameLength);
        }

        public AnimationChain GetAnimationChain(string chainName)
        {
            if (string.IsNullOrEmpty(chainName) || AnimationChains == null)
                return null;

            foreach (var chain in AnimationChains)
            {
                if (chain.Name == chainName)
                    return chain;
            }

            return null;
        }

        public bool ContainsAnimationChain(string chainName)
        {
            return GetAnimationChain(chainName) != null;
        }
    }
}