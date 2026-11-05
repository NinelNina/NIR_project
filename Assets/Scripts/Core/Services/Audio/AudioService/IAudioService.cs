using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public interface IAudioService
    {
        void Initialize();
        UniTask PlaySfx(string soundKey);
        void PlayUiSfx(string uiSoundKey);
        UniTask PlayVoiceOver(string characterId, string lineId);
        UniTask PlayMusic(string musicKey);
        void StopMusic();
        void SetMasterVolume(float volume);
        void SetSfxVolume(float volume);
        void SetMusicVolume(float volume);
    }
}