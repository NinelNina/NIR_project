using Core.Services;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;

public class GameplayState : IGameState
{
    [Inject] private SceneService _sceneService;
    [Inject] private AudioService _audioService;
    [Inject] private GameStateService _gameStateService;

    public async UniTask Enter()
    {
        Debug.Log("*** GameplayState.Enter()");

        if (_gameStateService.GetNewGameType())
        {
            await LoadGameLevel(true);
        }
        else
        {
            await LoadGameLevel(false, "что-то от сохранения");
        }

        await UniTask.Delay(1000);
        // Включаем музыку
        await _audioService.PlayMusic("FirstLevel"); //Потом поменять
    }

    public async UniTask LoadGameLevel(bool isNewGame, string LevelName = default)
    {
        await UniTask.Delay(300);

        if (isNewGame)
        {
            await _sceneService.LoadSceneAsync("LevelSandbox"); // потом поменять
            await _sceneService.UnloadSceneAsync("MainMenu");
        }
        else
        {
            await _sceneService.LoadSceneAsync(LevelName);
            await _sceneService.UnloadSceneAsync("MainMenu");
        }
    }

    public async UniTask Exit()
    {
        //пока ничего
        await UniTask.Delay(100);
        await UniTask.Yield();
    }
}