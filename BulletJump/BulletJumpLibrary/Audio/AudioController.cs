using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJumpLibrary.Audio
{
    public class AudioController : IDisposable
    {
        // Создаются экземпляры звуковых эффектов, которые можно ставить на паузу, снимать с паузы и/или удалять./or disposed.
        private readonly List<SoundEffectInstance> _activeSoundEffectInstances;

        // Отслеживает громкость воспроизведения песни при отключении и включении звука.
        private float _previousSongVolume;

        // Отслеживает громкость воспроизведения звуковых эффектов при отключении и включении звука.
        private float _previousSoundEffectVolume;

        /// <summary>
        /// Возвращает значение, указывающее, отключен ли звук.
        /// </summary>
        public bool IsMuted { get; private set; }

        /// <summary>
        /// Возвращает или устанавливает общую громкость песен.
        /// </summary>
        /// <remarks>
        /// Если значение IsMuted равно true, то средство получения всегда будет возвращать значение 0.0f, а
        ///средство настройки 
        /// будет игнорировать настройку громкости.
                           /// </remarks>
        public float SongVolume
        {
            get
            {
                if (IsMuted)
                {
                    return 0.0f;
                }

                return MediaPlayer.Volume;
            }
            set
            {
                if (IsMuted)
                {
                    return;
                }

                MediaPlayer.Volume = Math.Clamp(value, 0.0f, 1.0f);
            }
        }

        /// <summary>
        /// Получает или устанавливает общую громкость звуковых эффектов.
        /// </summary>
        /// <remarks>
        /// Если значение IsMuted равно true, то средство получения всегда будет возвращать значение 0.0f, а
        /// средство настройки 
        /// будет игнорировать настройку громкости.
        /// </remarks>
        public float SoundEffectVolume
        {
            get
            {
                if (IsMuted)
                {
                    return 0.0f;
                }

                return SoundEffect.MasterVolume;
            }
            set
            {
                if (IsMuted)
                {
                    return;
                }

                SoundEffect.MasterVolume = Math.Clamp(value, 0.0f, 1.0f);
            }
        }

        /// <summary>
        ///  Возвращает значение, указывающее, был ли удален этот аудиоконтроллер.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Создает новый экземпляр аудиоконтроллера.
        /// </summary>
        public AudioController()
        {
            _activeSoundEffectInstances = new List<SoundEffectInstance>();
        }

        // Финализатор вызывается, когда объект собирается сборщиком мусора.
        ~AudioController() => Dispose(false);

        /// <summary>
        /// Updates this audio controller.
        /// </summary>
        public void Update()
        {
            for (int i = _activeSoundEffectInstances.Count - 1; i >= 0; i--)
            {
                SoundEffectInstance instance = _activeSoundEffectInstances[i];

                if (instance.State == SoundState.Stopped)
                {
                    if (!instance.IsDisposed)
                    {
                        instance.Dispose();
                    }
                    _activeSoundEffectInstances.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Воспроизводит заданный звуковой эффект.
        /// </summary>
        /// <param name="soundEffect">The sound effect to play.</param>
        /// <returns>Экземпляр звукового эффекта, созданный с помощью этого метода.</returns>
        public SoundEffectInstance PlaySoundEffect(SoundEffect soundEffect)
        {
            return PlaySoundEffect(soundEffect, 1.0f, 0.0f, 0.0f, false);
        }

        /// <summary>
        /// Plays the given sound effect with the specified properties.
        /// </summary>
        /// <param name="soundEffect">The sound effect to play.</param>
        /// <param name="volume">The volume, ranging from 0.0 (silence) to 1.0 (full volume).</param>
        /// <param name="pitch">Регулировка высоты звука в диапазоне от -1,0 (понижение на октаву) до 0,0 (без изменений) и 1,0 (повышение на октаву)</param>
        /// <param name="pan">Панорамирование в диапазоне от -1,0 (левый динамик) до 0,0 (по центру), 1,0 (правый динамик).</param>
        /// <param name="isLooped">Whether the the sound effect should loop after playback.</param>
        /// <returns>The sound effect instance created by playing the sound effect.</returns>
        /// <returns>The sound effect instance created by this method.</returns>
        public SoundEffectInstance PlaySoundEffect(SoundEffect soundEffect, float volume, float pitch, float pan, bool isLooped)
        {
            // Создайте экземпляр на основе заданного звукового эффекта.
            SoundEffectInstance soundEffectInstance = soundEffect.CreateInstance();

            // Примените указанные значения громкости, высоты тона, панорамирования и цикла.
            soundEffectInstance.Volume = volume;
            soundEffectInstance.Pitch = pitch;
            soundEffectInstance.Pan = pan;
            soundEffectInstance.IsLooped = isLooped;

            // Скажите экземпляру, чтобы он играл
            soundEffectInstance.Play();

            // Добавьте его в активные экземпляры для отслеживания
            _activeSoundEffectInstances.Add(soundEffectInstance);

            return soundEffectInstance;
        }

        /// <summary>
        /// Plays the given song.
        /// </summary>
        /// <param name="song">The song to play.</param>
        /// <param name="isRepeating">Optionally specify if the song should repeat.  Default is true.</param>
        public void PlaySong(Song song, bool isRepeating = true)
        {
            // Проверьте, воспроизводится ли медиаплеер, если да, остановите его.
            // Если мы не остановим воспроизведение, это может вызвать проблемы на некоторых платформах
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }

            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = isRepeating;
        }

        /// <summary>
        /// Pauses all audio.
        /// </summary>
        public void PauseAudio()
        {
            // Pause any active songs playing.
            MediaPlayer.Pause();

            // Pause any active sound effects.
            foreach (SoundEffectInstance soundEffectInstance in _activeSoundEffectInstances)
            {
                soundEffectInstance.Pause();
            }
        }

        /// <summary>
        /// Resumes play of all previous paused audio.
        /// </summary>
        public void ResumeAudio()
        {
            // Resume paused music
            MediaPlayer.Resume();

            // Resume any active sound effects.
            foreach (SoundEffectInstance soundEffectInstance in _activeSoundEffectInstances)
            {
                soundEffectInstance.Resume();
            }
        }

        /// <summary>
        /// Mutes all audio.
        /// </summary>
        public void MuteAudio()
        {
            // Store the volume so they can be restored during ResumeAudio
            _previousSongVolume = MediaPlayer.Volume;
            _previousSoundEffectVolume = SoundEffect.MasterVolume;

            // Set all volumes to 0
            MediaPlayer.Volume = 0.0f;
            SoundEffect.MasterVolume = 0.0f;

            IsMuted = true;
        }

        /// <summary>
        /// Unmutes all audio to the volume level prior to muting.
        /// </summary>
        public void UnmuteAudio()
        {
            // Restore the previous volume values.
            MediaPlayer.Volume = _previousSongVolume;
            SoundEffect.MasterVolume = _previousSoundEffectVolume;

            IsMuted = false;
        }

        /// <summary>
        /// Toggles the current audio mute state.
        /// </summary>
        public void ToggleMute()
        {
            if (IsMuted)
            {
                UnmuteAudio();
            }
            else
            {
                MuteAudio();
            }
        }

        /// <summary>
        /// Позволяет избавиться от этого аудиоконтроллера и очистить ресурсы.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Удаляет этот аудиоконтроллер и очищает ресурсы.
        /// </summary>
        /// <param name="disposing">Указывает, следует ли утилизировать управляемые ресурсы.</param>
        protected void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (SoundEffectInstance soundEffectInstance in _activeSoundEffectInstances)
                {
                    soundEffectInstance.Dispose();
                }
                _activeSoundEffectInstances.Clear();
            }

            IsDisposed = true;
        }
    }
}
