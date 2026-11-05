using UnityEngine;
using UnityEngine.SceneManagement;
using Reflex.Attributes;
using Core.Services;

public class UIManager : MonoBehaviour
{
    [Inject] private GameStateService _gameStateService;
    [Inject] private SceneService _sceneService;
    //[Inject] private SaveService _saveService; //на будущее

    public async void NewGame()
    {
        _gameStateService.SetNewGame(true);
        await _gameStateService.SetState(GameState.Gameplay);
    }

    public void LoadGame()
    {
        //что-то с сохранениями
    }

    public async void RestartLevel()
    {
        await _sceneService.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        //пока так
        Application.Quit();
    }
}
