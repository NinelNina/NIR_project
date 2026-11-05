using System.Collections.Generic;
using UnityEngine;

    [CreateAssetMenu(menuName = "Configs/Scene Loader Config")]
    public class SceneLoaderConfig : ScriptableObject
    {
        [Header("Настройки загрузки")]
        public float minLoadingTime = 2f;          // Минимальное время загрузки
        public float fadeDuration = 0.5f;          // Длительность затемнения
    
        [Header("Сцены игры")]
        public List<SceneInfo> scenes = new List<SceneInfo>();
    
        [Header("Загрузочный экран")]
        public string loadingSceneName = "LoadingScene";
    }

    [System.Serializable]
    public class SceneInfo
    {
        public string sceneId;          // "main_menu", "level_01"
        public string sceneName;        // "MainMenu", "Level01" 
        public SceneType type;          // MainMenu, Level, Cutscene
        public string displayName;      // "Главное меню"
        public string nextSceneId;      // Куда переходить после
        public bool showLoadingScreen = true;
    }

    public enum SceneType
    {
        Bootstrap,
        IntroLogo,
        IntroVideo,
        MainMenu,
        LoadingScreen,
        Level,
        Cutscene,
    }