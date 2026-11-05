using Characters.Player;
using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HSM.Player
{
    [RequireComponent (typeof(DefaultMovement), typeof(Rigidbody2D))]
    public class PlayerStateDriver : MonoBehaviour
    {
        [SerializeField] private PlayerContextHandler _contextHandler;

        private StateMachine _machine;
        private State _root;
        private PlayerContext _playerContext;

        private string _lastPath;

        private void Awake()
        {
            _playerContext = (PlayerContext)_contextHandler.Context;
            SetupHSM();
        }

        private void Update()
        {
            _machine.Tick(Time.deltaTime);

            var path = StatePath(_machine.Root.Leaf());
            if (path != _lastPath)
            {
                //Debug.Log($"State: {path}");
                _lastPath = path;
            }
        }

        private void FixedUpdate()
        {
            
        }

        private static string StatePath(State s)
        {
            return string.Join(" > ", s.PathToRoot().Reverse().Select(n => n.GetType().Name));
        }

        private void SetupHSM()
        {
            _machine = new StateMachine();
            _root = new PlayerRoot(_machine, _playerContext);
            _machine.SetRoot(_root);
            var builder = new StateMachineBuilder(_machine, _root);
            builder.Build();
        }
    }
}