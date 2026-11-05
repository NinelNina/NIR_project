using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Characters.AntSquire
{
    public class AntSquireContextHandler : CharacterContextHandler
    {
        [SerializeField] private DefaultMovement _movement;
        [SerializeField] private BasePlatformEndChecker _platformEndChecker;
        [SerializeField] private BaseDetector _detector;

        [SerializeField] private MeleeAttackAbility _meleeAttack;
        [SerializeField] private ShootAbility _rangedAttack;

        public override CharacterContext Context => _context;

        protected AntSquireContext _context = new AntSquireContext();

        protected override void SetupContext()
        {
            _context.Movement = _movement;
            _context.PlatformEndChecker = _platformEndChecker;
            _context.Detector = _detector;
            _context.RangedAttack = _rangedAttack;
            _context.MeleeAttack = _meleeAttack;

            _detector.StateChangedEvent.AddListener((bool state) =>
            {
                if (state)
                {
                    _context.Target = _detector.DetectedObjects[0];
                }
                else
                {
                    _context.Target = null;
                }
            });
        }
    }
}
