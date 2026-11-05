using Core.Services;
using Reflex.Attributes;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BootController : MonoBehaviour
{
    [Inject] private GameStateService _gameStateService;

    private async void Awake()
    {
        //Загружаем в Dictionary<GameState, IGameState> все доступные состояния игры
        _gameStateService.UploadGameStates();

        //Запускаем состояние Boot
        await _gameStateService.SetState(GameState.Boot);
    }
}
