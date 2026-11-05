using Characters.Player;
using HSM;
using HSM.Player;
using UnityEngine;

namespace HSM.Player
{

    public class PlayerRoot : State
    {
        public readonly Grounded Grounded;
        public readonly Airborne Airborne;

        private PlayerContext _context;

        public PlayerRoot (StateMachine m, PlayerContext context) : base(m, null)
        {
            _context = context;
            Grounded = new Grounded(m, this, _context);
            Airborne = new Airborne(m, this, _context);
        }

        public override State GetInitialState()
        {
            return Grounded;
        }

        protected override State GetTransition()
        {
            if (_context.IsGrounded)
            {
                return Grounded;
            }
            return Airborne;
        }
    }
}