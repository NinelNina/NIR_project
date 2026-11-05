using Core.Services;
using Core.Graphics;
using Cysharp.Threading.Tasks;
using Reflex.Core;
using Reflex.Attributes;
using UnityEngine;
using Reflex.Injectors;

public class BootState : IGameState
{
    [Inject] private SettingsService _settingsService;
    [Inject] private GraphicsService _graphicsService;
    [Inject] private AudioService _audioService;
    [Inject] private LocalizationService _localizationService;
    [Inject] private GameStateService _gameStateService;

    public async UniTask Enter()
    {
        Debug.Log("🔌 BootState.Enter()");

        // 1. Загружаем настройки из PlayerPrefs
        _settingsService.Load();

        // 2. Применяем графические и аудио настройки
        ApplySettingsToAllSystems();

        // 3. Инициализируем локализацию
        _localizationService.ApplyLanguage(_settingsService.Current);

        // 4. Проверяем наличие сохранений (опционально)
        //var hasSave = await _saveService.HasSaveAsync(0);
        //Debug.Log($"Save slot 0: {(hasSave ? "exists" : "empty")}");

        // 5. Небольшая задержка для стабильности (опционально)
        await UniTask.Delay(300);

        // 6. Переходим в главное меню
        await _gameStateService.SetState(GameState.MainMenu);
    }

    private void ApplySettingsToAllSystems()
    {
        //Inject зависимостей в объекты, созданные в RunTime (в том числе те, что от RootInstaller)
        AttributeInjector.Inject(_audioService, Container.RootContainer);
        AttributeInjector.Inject(_graphicsService, Container.RootContainer);
        AttributeInjector.Inject(_localizationService, Container.RootContainer);

        var settings = new GameSettingsData();
        

        _audioService.Initialize();
        _audioService.SetMasterVolume(settings.MasterVolume);
        _audioService.SetMusicVolume(settings.MusicVolume);
        _audioService.SetSfxVolume(settings.SFXVolume);

        var graphics = new GraphicsSettingsData();
        _graphicsService.ApplyGraphicsSettings(graphics);

        _localizationService.ApplyLanguage(settings);

        _settingsService.ApplyNewSettings(settings);
    }

    // ВАЖНО: Отписываемся при уничтожении объекта
    private void OnDestroy()
    {
        ISettingsService.OnSettingsChanged -= ApplySettingsToAllSystems;
    }

    public async UniTask Exit()
    {
        Debug.Log("BootState.Exit()");

        // В BootState обычно нечего очищать, но для симметрии:
        await UniTask.Yield();
    }
}