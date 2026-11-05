using Characters;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "UtilityAI/Action/PatrolPlatformAction")]
    public class PatrolPlatformAction : AIAction
    {
        [Range(-1, 1)] public int MovementDirection = -1;

        private DefaultMovement _movement;
        private BasePlatformEndChecker _platfromEndChecker;

        public override void Initialize(CharacterContext context)
        {
            base.Initialize(context);

            if (context is IMove)
            {
                _movement = (context as IMove).Movement;
            }
            if (context is ICheckPlatformEnd)
            {
                _platfromEndChecker = (context as ICheckPlatformEnd).PlatformEndChecker;
            }
        }

        public override void Execute(CharacterContext context, float deltaTime)
        {
            if (_movement == null)
            {
                return;
            }

            ChooseDirection();
            _movement.SetDirection(MovementDirection);
        }

        private void ChooseDirection()
        {
            if (MovementDirection < 0)
            {
                MovementDirection = -1;
            }
            else if (MovementDirection > 0)
            {
                MovementDirection = 1;
            }

            if (_platfromEndChecker == null)
            {
                return;
            }

            bool hasGroundAhead = _platfromEndChecker.CheckRight(), hasGroundBehind = _platfromEndChecker.CheckLeft();
            if (hasGroundAhead)
            {
                if (MovementDirection == 0)
                {
                    MovementDirection = 1;
                }
                return;
            }
            if (hasGroundBehind)
            {
                MovementDirection = -MovementDirection;
            }
            else
            {
                MovementDirection = 0;
            }
        }
    }
}
