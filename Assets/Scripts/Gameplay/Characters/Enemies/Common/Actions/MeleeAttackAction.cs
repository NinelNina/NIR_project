using Characters;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "UtilityAI/Action/MeleeAttackAction")]
    public class MeleeAttackAction : AIAction
    {
        private DefaultMovement _movement;
        private BasePlatformEndChecker _platfromEndChecker;
        private MeleeAttackAbility _meleeAttack;

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
            if (context is IMeleeAttack)
            {
                _meleeAttack = (context as IMeleeAttack).MeleeAttack;
            }
        }

        public override void Execute(CharacterContext context, float deltaTime)
        {
            if (_movement != null)
            {
                _movement.SetDirection(0);
            }

            if (_meleeAttack == null)
            {
                return;
            }

            _meleeAttack.TryPerform();
        }
    }
}
