using UnityEngine;
using HSM;
using System.Collections.Generic;

namespace HSM
{
    public abstract class State
    {
        public readonly StateMachine Machine;
        public readonly State Parent;

        public bool IsActive => (Parent == null || Parent.ActiveChild == this);
        public bool IsInitialState => (Parent != null && Parent.GetInitialState() == this);
        public bool IsLocked { get; protected set; } = false;
        public bool IsRestricted { get; protected set; } = false;

        public State ActiveChild { get; private set; }

        public List<State> Children { get; private set; }

        public IReadOnlyList<IActivity> Activities => _activities;

        private readonly List<IActivity> _activities = new List<IActivity>();

        public State(StateMachine machine, State parent = null)
        {
            Machine = machine;
            Parent = parent;
        }

        public State Leaf()
        {
            State s = this;
            while (s.ActiveChild != null)
            {
                s = s.ActiveChild;
            }
            return s;
        }

        public IEnumerable<State> PathToRoot()
        {
            for (State s = this; s != null;  s = s.Parent)
            {
                yield return s;
            }
        }

        public void AddActivity(IActivity a)
        {
            if (a != null)
            {
                _activities.Add(a);
            }
        }

        public void SetChildren(List<State> children)
        {
            Children = new List<State>(children);
        }

        public void SetLocked(bool locked)
        {
            if (locked == true && !IsActive)
            {
                return;
            }
            IsLocked = locked;
        }

        public void SetRestricted(bool restricted)
        {
            if (restricted == false)
            {
                IsRestricted = false;
                var initialState = GetInitialState();
                if (initialState != null)
                {
                    initialState.SetRestricted(false);
                }
                return;
            }
            if (IsActive)
            {
                return;
            }
            if (IsInitialState)
            {
                Parent.SetRestricted(restricted);
            }
            else
            {
                IsRestricted = restricted;
            }
        }

        public virtual State GetInitialState()
        {
            return null;
        }

        protected virtual State GetTransition()
        {
            return null;
        }

        protected virtual void OnEnter()
        {

        }

        protected virtual void OnExit()
        {

        }

        protected virtual void OnUpdate(float deltaTime)
        {

        }

        internal void Enter(bool setInitialState = true)
        {
            if (Parent != null)
            {
                Parent.ActiveChild = this;
            }
            OnEnter();

            State init = GetInitialState();
            if (init != null && setInitialState)
            {
                Machine.RequestTransition(this, init);
            }
        }

        internal void Exit()
        {
            if (ActiveChild != null)
            {
                ActiveChild.Exit();
            }
            ActiveChild = null;
            OnExit();
        }

        internal void Update(float deltaTime)
        {
            State t = GetTransition();
            if (t != null && t != this && t != ActiveChild)
            {
                Machine.RequestTransition(Leaf(), t);
                return;
            }

            if (ActiveChild != null)
            {
                ActiveChild.Update(deltaTime); 
            }
            OnUpdate(deltaTime);
        }
    }
}
