using Characters;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "UtilityAI/Action/RangedAttackAction")]
    public class RangedAttackAction : AIAction
    {
        private DefaultMovement _movement;
        private BasePlatformEndChecker _platfromEndChecker;
        private ShootAbility _rangedAttack;

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
            if (context is IRangedAttack)
            {
                _rangedAttack = (context as IRangedAttack).RangedAttack;
            }
        }

        public override void Execute(CharacterContext context, float deltaTime)
        {
            if (_movement != null)
            {
                _movement.SetDirection(0);
            }

            if (_rangedAttack == null)
            {
                return;
            }

            if (!(context is ITargeting))
            {
                return;
            }

            GameObject target = (context as ITargeting).Target;

            _rangedAttack.TryShoot(target);
        }
    }
}
