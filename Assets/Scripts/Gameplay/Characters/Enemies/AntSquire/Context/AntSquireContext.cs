using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Characters.AntSquire
{
    [Serializable]
    public class AntSquireContext : CharacterContext, ICheckPlatformEnd, IDetect, IMove, ITargeting, IMeleeAttack, IRangedAttack
    {
        public MeleeAttackAbility MeleeAttack { get; set; }

        public ShootAbility RangedAttack { get; set; }

        public GameObject Target { get; set; }

        public BasePlatformEndChecker PlatformEndChecker { get; set; }

        public BaseDetector Detector { get; set; }

        public DefaultMovement Movement { get; set; }
    }
}
