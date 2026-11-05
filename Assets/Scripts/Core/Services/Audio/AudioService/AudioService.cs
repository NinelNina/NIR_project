using UnityEngine;
using UnityEngine.Audio;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public class AudioService : IAudioService
    {
        private AudioConfig _config;
        private AudioMixer _mixer;
        [Inject] AudioLibrary _audioLibrary;

        private AudioSource _sfxSource;
        private AudioSource _musicSource;
        private AudioSource _voiceSource; // Отдельный AudioSource для озвучки!

        // Инициализация конфигом (вызывается один раз при старте)
        public async void Initialize()
        {
            await UniTask.Delay(100);
            _config = ConfigContainer.Instance.GetAudioConfig();
            _mixer = _config.AudioMixer;
            ApplyAllVolumes();
        }

        public async UniTask PlayVoiceOver(string characterId, string lineId)
        {
            if (_voiceSource == null) return;
            // Применяем громкость: Общая * Громкость озвучки * Множитель
            float finalVolume = _config.MasterVolume * _config.VoiceVolume;

            var clip = await _audioLibrary.GetVoiceLineAsync(characterId, lineId);
            if (clip != null)
            {
                _voiceSource.PlayOneShot(clip, finalVolume);
            }
            
        }

        // --- Публичные методы сервиса ---
        public async UniTask PlaySfx(string soundKey)
        {
            if (_sfxSource == null) return;

            float finalVolume = _config.MasterVolume * _config.SfxVolume;

            var clip = await _audioLibrary.GetSfxAsync(soundKey);

            if (clip != null)
            {
                _sfxSource.PlayOneShot(clip, finalVolume);
            }
        }

        public void PlayUiSfx(string uiSoundKey)
        {
            switch (uiSoundKey)
            {
                case "button_click":
                    if (_config.ButtonClick != null)
                        _sfxSource.PlayOneShot(_config.ButtonClick);
                    break;
                case "menu_open":
                    if (_config.MenuOpen != null)
                        _sfxSource.PlayOneShot(_config.MenuOpen);
                    break;
                case "menu_close":
                    if (_config.MenuClose != null)
                        _sfxSource.PlayOneShot(_config.MenuClose);
                    break;
            }
        }

        public async UniTask PlayMusic(string musicKey)
        {
            if (_musicSource == null) return;

            float finalVolume = _config.MasterVolume * _config.MusicVolume;

            var clip = await _audioLibrary.GetMusicAsync(musicKey);

            if (clip != null)
            {
                _musicSource.clip = clip;
                _musicSource.volume = finalVolume;
                _musicSource.Play();
            }
        }

        public void StopMusic()
        {
            _musicSource.Stop();
        }

        public async void SetMasterVolume(float volume)
        {
            await UniTask.Delay(150);
            _config.MasterVolume = Mathf.Clamp01(volume);
            _mixer.SetFloat("Master", LinearToDecibels(_config.MasterVolume));
        }

        public async void SetSfxVolume(float volume)
        {
            await UniTask.Delay(150);
            _config.SfxVolume = Mathf.Clamp01(volume);
            _mixer.SetFloat("SfxVol", LinearToDecibels(_config.SfxVolume));
        }

        public async void SetMusicVolume(float volume)
        {
            await UniTask.Delay(150);
            _config.MusicVolume = Mathf.Clamp01(volume);
            _mixer.SetFloat("MusicVol", LinearToDecibels(_config.MusicVolume));
        }

        public async void SetVoiceVolume(float volume)
        {
            await UniTask.Delay(150);
            _config.VoiceVolume = Mathf.Clamp01(volume);
            _mixer.SetFloat("VoiceVol", LinearToDecibels(_config.VoiceVolume));
        }

        // --- Приватные вспомогательные методы ---
        private void ApplyAllVolumes()
        {
            SetMasterVolume(_config.MasterVolume);
            SetSfxVolume(_config.SfxVolume);
            SetMusicVolume(_config.MusicVolume);
            SetVoiceVolume(_config.VoiceVolume);
        }

        // Конвертация линейного значения громкости (0..1) в децибелы для AudioMixer
        private float LinearToDecibels(float linear)
        {
            return Mathf.Approximately(linear, 0f) ? -80f : Mathf.Log10(linear) * 20f;
        }
    }
}