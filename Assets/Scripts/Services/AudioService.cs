// AudioService.cs — Audio playback service for Bloomline
using UnityEngine;
using Bloomline.Core;
using Bloomline.Data;

namespace Bloomline.Services
{
    /// <summary>
    /// Interface for audio playback (SFX and music).
    /// </summary>
    public interface IAudioService
    {
        /// <summary>Play a one-shot sound effect.</summary>
        void PlaySFX(AudioClip clip);

        /// <summary>Play background music (looping).</summary>
        void PlayMusic(AudioClip clip);

        /// <summary>Stop the currently playing music.</summary>
        void StopMusic();

        /// <summary>Refresh volume/mute settings from GameSettings.</summary>
        void ApplySettings(GameSettings settings);

        // Convenience methods for common sound effects
        void PlayTileRotate();
        void PlayLightConnect();
        void PlayFlowerBloom();
        void PlayLevelComplete();
        void PlayButtonTap();
        void PlayLockedTile();
    }

    /// <summary>
    /// MonoBehaviour audio service using two AudioSources (SFX + Music).
    /// Respects GameSettings for sound/music toggle and volume.
    /// Null-safe — works even when no AudioClips are assigned.
    /// </summary>
    public class AudioService : MonoBehaviour, IAudioService
    {
        [Header("Audio Clips")]
        [SerializeField] private AudioClip tileRotate;
        [SerializeField] private AudioClip lightConnect;
        [SerializeField] private AudioClip flowerBloom;
        [SerializeField] private AudioClip levelComplete;
        [SerializeField] private AudioClip buttonTap;
        [SerializeField] private AudioClip lockedTile;

        private AudioSource _sfxSource;
        private AudioSource _musicSource;
        private bool _soundEnabled = true;
        private bool _musicEnabled = true;
        private float _soundVolume = 1f;
        private float _musicVolume = 0.7f;

        /// <summary>Quick accessor for the tile-rotate clip.</summary>
        public AudioClip TileRotateClip => tileRotate;
        /// <summary>Quick accessor for the light-connect clip.</summary>
        public AudioClip LightConnectClip => lightConnect;
        /// <summary>Quick accessor for the flower-bloom clip.</summary>
        public AudioClip FlowerBloomClip => flowerBloom;
        /// <summary>Quick accessor for the level-complete clip.</summary>
        public AudioClip LevelCompleteClip => levelComplete;
        /// <summary>Quick accessor for the button-tap clip.</summary>
        public AudioClip ButtonTapClip => buttonTap;
        /// <summary>Quick accessor for the locked-tile clip.</summary>
        public AudioClip LockedTileClip => lockedTile;

        private void Awake()
        {
            // Create SFX AudioSource
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;

            // Create Music AudioSource
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.volume = _musicVolume;

            // Load settings if available
            if (ServiceLocator.Has<ISaveService>())
            {
                GameSettings settings = ServiceLocator.Get<ISaveService>().LoadSettings();
                ApplySettings(settings);
            }
        }

        /// <inheritdoc/>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || !_soundEnabled || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, _soundVolume);
        }

        /// <inheritdoc/>
        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || _musicSource == null) return;

            // Don't restart if the same clip is already playing
            if (_musicSource.clip == clip && _musicSource.isPlaying) return;

            _musicSource.clip = clip;
            _musicSource.volume = _musicEnabled ? _musicVolume : 0f;
            _musicSource.Play();
        }

        /// <inheritdoc/>
        public void StopMusic()
        {
            if (_musicSource != null && _musicSource.isPlaying)
            {
                _musicSource.Stop();
            }
        }

        /// <inheritdoc/>
        public void ApplySettings(GameSettings settings)
        {
            if (settings == null) return;

            _soundEnabled = settings.soundEnabled;
            _musicEnabled = settings.musicEnabled;
            _soundVolume = settings.soundVolume;
            _musicVolume = settings.musicVolume;

            if (_musicSource != null)
            {
                _musicSource.volume = _musicEnabled ? _musicVolume : 0f;
            }
        }

        public void PlayTileRotate() { PlaySFX(tileRotate); }
        public void PlayLightConnect() { PlaySFX(lightConnect); }
        public void PlayFlowerBloom() { PlaySFX(flowerBloom); }
        public void PlayLevelComplete() { PlaySFX(levelComplete); }
        public void PlayButtonTap() { PlaySFX(buttonTap); }
        public void PlayLockedTile() { PlaySFX(lockedTile); }
    }
}
