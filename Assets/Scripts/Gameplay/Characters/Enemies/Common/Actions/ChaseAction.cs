using Characters;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "UtilityAI/Action/ChaseAction")]
    public class ChaseAction : AIAction
    {
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
            if (_movement == null || !(context is ITargeting))
            {
                return;
            }

            Vector3 targetPos = (context as ITargeting).Target.transform.position;
            Vector3 selfPos = context.CharacterObject.transform.position;

            float desiredDirection = 1;

            if (selfPos.x > targetPos.x)
            {
                desiredDirection = -1;
            }
            _movement.SetDirection(desiredDirection);
        }

        //private void ChooseDirection()
        //{
        //    if (MovementDirection < 0)
        //    {
        //        MovementDirection = -1;
        //    }
        //    else if (MovementDirection > 0)
        //    {
        //        MovementDirection = 1;
        //    }

        //    if (_platfromEndChecker == null)
        //    {
        //        return;
        //    }

        //    bool hasGroundAhead = _platfromEndChecker.CheckRight(), hasGroundBehind = _platfromEndChecker.CheckLeft();
        //    if (hasGroundAhead)
        //    {
        //        if (MovementDirection == 0)
        //        {
        //            MovementDirection = 1;
        //        }
        //        return;
        //    }
        //    if (hasGroundBehind)
        //    {
        //        MovementDirection = -MovementDirection;
        //    }
        //    else
        //    {
        //        MovementDirection = 0;
        //    }
        //}
    }
}
