using Characters;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "UtilityAI/Consideration/Constant")]
    public class ConstantConsideration : Consideration
    {
        [Range(0f, 1f)] public float Value;

        public override float Evaluate(CharacterContext context) => Value;
    }
}
