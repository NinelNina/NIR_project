using Characters.Player;
using HSM;
using HSM.Player;
using UnityEngine;

namespace HSM.Player
{
    public class Move : State
    {
        private PlayerContext _context;

        public Move(StateMachine m, State parent, PlayerContext context) : base(m, parent)
        {
            _context = context;
        }

        protected override State GetTransition()
        {
            
            return null;
        }

        protected override void OnUpdate(float deltaTime)
        {
            
        }
    }
}