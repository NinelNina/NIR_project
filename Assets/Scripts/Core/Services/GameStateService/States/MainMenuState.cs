using Core.Services;
// using Core.UI;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;

public class MainMenuState : IGameState
{
    public GameState StateType => GameState.MainMenu;

    [Inject] private SceneService _sceneService;
    [Inject] private AudioService _audioService;
    //[Inject] private UIService _uiService;

    public async UniTask Enter()
    {
        Debug.Log("🏠 MainMenuState.Enter()");

        // 1. Загружаем сцену меню (если ещё не загружена)
        if (!_sceneService.IsSceneLoaded("MainMenu"))
        {
            await LoadMenu();
        }

        // 3. Включаем музыку меню
        //await _audioService.PlayMusic("MainMenu");

        // 4. Показываем главное меню
        //  await _uiService.ShowScreenAsync(ScreenType.MainMenu);
    }

    private async UniTask LoadMenu()
    {
        await UniTask.Delay(100); // Используем UniTask вместо System.Threading.Tasks
        await _sceneService.LoadSceneAsync("MainMenu");
        await _sceneService.UnloadSceneAsync("BootstrapScene");
    }

    public async UniTask Exit()
    {
        Debug.Log("🏠 MainMenuState.Exit()");
        await UniTask.Delay(100);

        // КРИТИЧНО: Отписываемся от событий!

        // Скрываем меню
        // await _uiService.HideScreenAsync(ScreenType.MainMenu);

        // Выключаем музыку (опционально, если в геймплее своя)
        // _audioService.StopMusic();

        await UniTask.Yield();
    }
}