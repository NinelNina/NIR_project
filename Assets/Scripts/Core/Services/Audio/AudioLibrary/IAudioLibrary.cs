
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Services
{
    public interface IAudioLibrary
    {
        UniTask<AudioClip> GetSfxAsync(string soundKey);
        UniTask<AudioClip> GetVoiceLineAsync(string characterId, string lineId);

        UniTask<AudioClip> GetMusicAsync(string musicKey);
    }
}