using UnityEngine;
using HSM;
using System.Collections.Generic;

namespace HSM
{
    public class StateMachine
    {
        public State Root { get; private set; }
        public readonly TransitionSequencer Sequencer;

        public bool IsFrozen { get; private set; } = false;

        private bool _started;

        public StateMachine(State root = null)
        {
            Root = root;
            Sequencer = new TransitionSequencer(this);
        }

        public State GetCurrentState()
        {
            return Root.Leaf();
        }

        public bool RequestTransition(State from, State to)
        {
            //Debug.Log($"Requesting transition from {from} to {to}.");
            if (IsFrozen)
            {
                return false;
            }
            if (from == to || to == null)
            {
                return false;
            }
            //Debug.Log($"Current leaf: {currentLeaf}, tail: {tail}.");

            State lca = TransitionSequencer.Lca(from, to);
            if (lca == null)
            {
                return false;
            }

            for (var s = from; s != lca && s != null; s = s.Parent)
            {
                if (s.IsLocked)
                {
                    return false;
                }
            }

            for (var s = to; s != lca && s != null; s = s.Parent)
            {
                if (s.IsRestricted)
                {
                    return false;
                }
            }

            Sequencer.OnBeginTransition(from, to);
            return true;
        }

        public void SetRoot(State root)
        {
            if (root == null)
            {
                return;
            }
            if (Root != null && root != Root)
            {
                Debug.LogWarning("It is prohibited to change the currently existing root state.");
                return;
            }
            Root = root;
        }

        public void Start()
        {
            if (_started)
            {
                return;
            }

            _started = true;
            Root.Enter();
        }

        public void Tick(float deltaTime)
        {
            if (!_started)
            {
                Start();
            }
            //InternalTick(deltaTime);
            Sequencer.Tick(deltaTime);
        }

        public void InternalTick(float deltaTime)
        {
            Root.Update(deltaTime);
        }

        public void SetFrozen(bool frozen)
        {
            IsFrozen = frozen;
        }
    }
}
