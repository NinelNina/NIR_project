using Characters.Player;
using HSM;
using HSM.Player;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HSM.Player
{
    public class Airborne : State
    {
        private PlayerContext _context;

        public Airborne(StateMachine m, State parent, PlayerContext context) : base(m, parent)
        {
            _context = context;
        }

        protected override State GetTransition()
        {
            if (_context.IsGrounded)
            {
                return ((PlayerRoot)Parent).Grounded;
            }
            return null;
        }

        protected override void OnUpdate(float deltaTime)
        {
            _context.Movement.SetDirection(_context.HorizontalDirection);
        }

        protected override void OnEnter()
        {
            base.OnEnter();
        }
    }
}