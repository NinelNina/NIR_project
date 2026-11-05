using Characters;
using UnityEngine;

namespace UtilityAI
{
    public abstract class AIAction : ScriptableObject
    {
        public Consideration Consideration;

        public virtual void Initialize(CharacterContext context)
        {

        }

        public float CalculateUtility(CharacterContext context) => Mathf.Clamp01(Consideration.Evaluate(context));

        public abstract void Execute(CharacterContext context, float deltaTime = 0f);

        public virtual bool IsLocked(CharacterContext context)
        {
            return false;
        }
    }
}