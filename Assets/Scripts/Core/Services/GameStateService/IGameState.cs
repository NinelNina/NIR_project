using Cysharp.Threading.Tasks;

public interface IGameState
{
    UniTask Enter();
    UniTask Exit();
}

public enum GameState
{
    None = 0,
    Boot = 1,
    MainMenu = 2,
    Gameplay = 3,
    Loading = 4,
    Pause = 5,
    GameOver = 6,
    Victory = 7
}