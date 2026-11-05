using UnityEngine;
using System;
using System.Collections;

namespace Characters.Player
{
    public class PlayerContextHandler : CharacterContextHandler
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private BaseChecker _groundChecker;

        public override CharacterContext Context => _context;

        private PlayerContext _context = new PlayerContext();

        private Rigidbody2D _rb;
        private DefaultMovement _movement;

        private void OnDestroy()
        {
            if (_groundChecker != null)
            {
                _groundChecker.StateChangedEvent.RemoveAllListeners();
            }
        }

        private void SetupComponents()
        {
            _rb = GetComponent<Rigidbody2D>();
            _movement = GetComponentInChildren<DefaultMovement>();
        }

        protected override void SetupContext()
        {
            SetupComponents();

            _context.Rb = _rb;
            _context.Movement = _movement;
            _context.Animator = _animator;

            _groundChecker.StateChangedEvent.AddListener((bool isGrounded) => _context.IsGrounded = isGrounded);
        }

    }
}
