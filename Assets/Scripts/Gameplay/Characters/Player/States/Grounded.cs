using Characters.Player;
using HSM;
using HSM.Player;
using UnityEngine;

namespace HSM.Player
{
    public class Grounded : State
    {
        public readonly Idle Idle;
        public readonly Move Move;

        private PlayerContext _context;

        public Grounded(StateMachine m, State parent, PlayerContext context) : base(m, parent)
        {
            _context = context;
            Idle = new Idle(m, this, _context);
            Move = new Move(m, this, _context);
        }

        public override State GetInitialState()
        {
            return Idle;
        }

        protected override void OnUpdate(float deltaTime)
        {
            _context.Movement.SetDirection(_context.HorizontalDirection);

            if (_context.IsJumpPressed)
            {
                _context.Movement.Jump();
            }
        }

        protected override State GetTransition()
        {
            return _context.IsGrounded ? null : ((PlayerRoot)Parent).Airborne;
        }

        protected override void OnEnter()
        {
            base.OnEnter();
        }

        protected override void OnExit()
        {
            //_context.Movement.SetDirection(0);
        }
    }
}