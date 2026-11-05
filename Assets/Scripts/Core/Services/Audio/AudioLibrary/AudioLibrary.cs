using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Services
{
    public class AudioLibrary : IAudioLibrary
    {
        private readonly Dictionary<string, AudioClip> _cache = new();

        public async UniTask<AudioClip> GetSfxAsync(string soundKey)
        {
            string address = $"SFX/{soundKey}";
            if (_cache.TryGetValue(address, out var cachedClip))
                return cachedClip;

            var operation = Addressables.LoadAssetAsync<AudioClip>(address);
            var clip = await operation;

            if (clip != null)
                _cache[address] = clip;

            return clip;
        }

        public async UniTask<AudioClip> GetMusicAsync(string musicKey)
        {
            string address = $"Music/{musicKey}";
            if (_cache.TryGetValue(address, out var cachedClip))
                return cachedClip;

            var operation = Addressables.LoadAssetAsync<AudioClip>(address);
            var clip = await operation;

            if (clip != null)
                _cache[address] = clip;

            return clip;
        }

        public async UniTask<AudioClip> GetVoiceLineAsync(string characterId, string lineId)
        {
            string address = $"Voices/{characterId}/{lineId}";
            if (_cache.TryGetValue(address, out var cachedClip))
                return cachedClip;

            var operation = Addressables.LoadAssetAsync<AudioClip>(address);
            var clip = await operation;

            if (clip != null)
                _cache[address] = clip;

            return clip;
        }
    }
}