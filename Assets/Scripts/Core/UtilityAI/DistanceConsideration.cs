using Characters;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "UtilityAI/Consideration/Distance")]
    public class DistanceConsideration : Consideration
    {
        public AnimationCurve Curve;

        public Vector3 OriginLocalPos = Vector3.zero;

        [Min(0f)]public float MinDist = 0f;
        [Min(float.Epsilon)]public float MaxDist = 3f;

        public override float Evaluate(CharacterContext context)
        {
            if (!(context is ITargeting))
            {
                return 0;
            }

            GameObject target = (context as ITargeting).Target;
            if (target == null)
            {
                return 0;
            }

            Vector3 originGlobalPos = context.CharacterObject.transform.localToWorldMatrix.MultiplyPoint3x4(OriginLocalPos);
            Vector3 targetPos = target.transform.position;

            float dist = Vector3.Distance(originGlobalPos, targetPos);
            if (dist < MinDist || dist > MaxDist)
            {
                return 0;
            }

            float normalizedDist = dist / MaxDist;
            return Mathf.Clamp01(Curve.Evaluate(normalizedDist));
        }
    }
}
