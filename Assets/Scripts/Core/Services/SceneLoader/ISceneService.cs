using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Core.Services
{
    public interface ISceneService
    {
        public static event Action<string> OnSceneLoaded;
        public static event Action<string> OnSceneUnloaded;

        UniTask LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single,
            Action<float> onProgress = null);

        UniTask LoadSceneAdditiveAsync(string sceneName, Action<float> onProgress = null);
        UniTask UnloadSceneAsync(string sceneName);

        string CurrentScene { get; }
        bool IsSceneLoaded(string sceneName);
        IReadOnlyList<string> GetLoadedScenes();
    }
}