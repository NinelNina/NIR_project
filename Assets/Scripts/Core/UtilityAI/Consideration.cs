using Characters;
using UnityEngine;

namespace UtilityAI
{
    public abstract class Consideration : ScriptableObject
    {
        public abstract float Evaluate(CharacterContext context);
    }
}