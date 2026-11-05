using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;
using System.Linq;
using Reflex.Injectors;
using Reflex.Core;

namespace Core.Services
{
    public class GameStateService
    {
        [Inject] private IEnumerable<IGameState> _gameStates;

        private Dictionary<int, IGameState> _states = new Dictionary<int, IGameState>();

        private GameState _currentState = 0;

        private bool _isNewGame = true;

        public void UploadGameStates()
        {
            int i = 1;

            List<IGameState> list = _gameStates.ToList();
            

            foreach (var gameState in list)
            {
                _states.Add(i, gameState);
                i++;
            }

            Debug.Log(_states.Count);
        }

        public async UniTask SetState(GameState newState)
        {
            if (_currentState != GameState.None)
            {
                await _states[(int)_currentState].Exit();
            }
            

            if (_states.TryGetValue((int)newState, out var state))
            {
                _currentState = newState;
                AttributeInjector.Inject(state, Container.RootContainer);

                await state.Enter();
            }
            else
            {
                Debug.Log("данного состояния не существует (почему-то)");
            }
        }

        public void SetNewGame(bool isNewGame)
        {
            _isNewGame = isNewGame;
        }

        public bool GetNewGameType()
        {
            return _isNewGame;
        }
    }
}