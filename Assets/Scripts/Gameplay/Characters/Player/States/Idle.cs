using Characters.Player;
using HSM;
using HSM.Player;
using UnityEngine;

namespace HSM.Player
{
    public class Idle : State
    {
        private PlayerContext _context;

        public Idle(StateMachine m, State parent, PlayerContext context) : base(m, parent)
        {
            _context = context;
        }

        protected override State GetTransition()
        {
            return Mathf.Abs(_context.HorizontalDirection) >= 0.01f? ((Grounded)Parent).Move: null;
        }

        protected override void OnEnter()
        {
            _context.Movement.SetDirection(0);
        }
    }
}