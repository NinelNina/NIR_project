using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Core.Services
{
    public class SceneService : ISceneService
    {
        public static event Action<string> OnSceneLoaded;
        public static event Action<string> OnSceneUnloaded;
        
        public string CurrentScene { get; private set; } = "";
        
        // Храним все загруженные сцены (для аддитивной загрузки)
        private readonly HashSet<string> _loadedScenes = new();

        public async UniTask LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action<float> onProgress = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneService] Scene name is empty");
                return;
            }

            try
            {
                // Проверяем, существует ли сцена в билде
                if (!SceneExistsInBuild(sceneName))
                {
                    Debug.LogError($"[SceneService] Scene '{sceneName}' not found in build settings");
                    return;
                }

                // Для Single: сначала загружаем новую, потом выгружаем старую
                if (mode == LoadSceneMode.Single)
                {
                    // Загружаем новую сцену
                    var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                    
                    while (!loadOp.isDone)
                    {
                        onProgress?.Invoke(loadOp.progress * 0.9f); // 0-90%
                        await UniTask.Yield();
                    }
                    
                    // Активируем новую сцену
                    var newScene = SceneManager.GetSceneByName(sceneName);
                    if (newScene.IsValid())
                    {
                        SceneManager.SetActiveScene(newScene);
                    }
                    
                    // Выгружаем старую
                    if (!string.IsNullOrEmpty(CurrentScene) && _loadedScenes.Contains(CurrentScene))
                    {
                        await UnloadSceneAsync(CurrentScene);
                    }
                    
                    CurrentScene = sceneName;
                }
                else
                {
                    // Additive загрузка
                    var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                    
                    while (!loadOp.isDone)
                    {
                        onProgress?.Invoke(loadOp.progress);
                        await UniTask.Yield();
                    }
                }
                
                // Обновляем список загруженных сцен
                _loadedScenes.Add(sceneName);
                
                // Уведомляем подписчиков
                OnSceneLoaded?.Invoke(sceneName);
                
                Debug.Log($"[SceneService] Loaded: {sceneName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneService] Failed to load scene '{sceneName}': {e.Message}");
                throw;
            }
        }

        public async UniTask LoadSceneAdditiveAsync(string sceneName, Action<float> onProgress = null)
        {
            await LoadSceneAsync(sceneName, LoadSceneMode.Additive, onProgress);
        }

        public async UniTask UnloadSceneAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return;

            try
            {
                var scene = SceneManager.GetSceneByName(sceneName);
                
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    _loadedScenes.Remove(sceneName);
                    return; // Уже выгружена или не существовала
                }

                var unloadOp = SceneManager.UnloadSceneAsync(sceneName);
                await unloadOp.ToUniTask();
                
                // Обновляем состояние
                _loadedScenes.Remove(sceneName);
                
                if (CurrentScene == sceneName)
                {
                    CurrentScene = "";
                }
                
                OnSceneUnloaded?.Invoke(sceneName);
                
                Debug.Log($"[SceneService] Unloaded: {sceneName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneService] Failed to unload scene '{sceneName}': {e.Message}");
                throw;
            }
        }

        public bool IsSceneLoaded(string sceneName)
        {
            return _loadedScenes.Contains(sceneName);
        }

        public IReadOnlyList<string> GetLoadedScenes()
        {
            return new List<string>(_loadedScenes);
        }

        // Проверка, есть ли сцена в настройках билда
        private bool SceneExistsInBuild(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name.Equals(sceneName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}