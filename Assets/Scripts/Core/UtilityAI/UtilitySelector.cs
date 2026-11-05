using Characters;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UtilityAI
{
    public class UtilitySelector : MonoBehaviour
    {
        [SerializeField] protected CharacterContextHandler _contextHandler;

        public CharacterContext Context => _contextHandler.Context;

        public List<AIAction> Actions;

        protected AIAction _currentAction;

        private void Start()
        {
            foreach (var action in Actions)
            {
                action.Initialize(Context);
            }
        }

        protected void Update()
        {
            HandleAction(Time.deltaTime);
        }

        protected virtual AIAction ChooseAction()
        {
            AIAction bestAction = null;
            float bestVal = float.NegativeInfinity;

            foreach (var action in Actions)
            {
                float val = action.CalculateUtility(Context);
                if (val > bestVal)
                {
                    bestAction = action;
                    bestVal = val;
                }
            }
            return bestAction;
        }

        private void HandleAction(float deltaTime)
        {
            if (_currentAction != null && _currentAction.IsLocked(Context))
            {
                return;
            }
            _currentAction = ChooseAction();
            if (_currentAction != null)
            {
                _currentAction.Execute(Context, deltaTime);
            }
        }
    }
}
