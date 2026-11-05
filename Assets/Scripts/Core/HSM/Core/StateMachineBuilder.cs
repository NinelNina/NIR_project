using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HSM
{
    public class StateMachineBuilder
    {
        public readonly State Root;
        public readonly StateMachine Machine;

        public StateMachineBuilder(StateMachine machine, State root)
        {
            Machine = machine;
            Root = root;
        }

        public void Build()
        {
            Wire(Root, Machine, new HashSet<State>());
        }

        private void Wire(State s, StateMachine m, HashSet<State> visited)
        {
            if (s == null)
            {
                return;
            }
            if (!visited.Add(s))
            {
                return;
            }

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

            var children = new List<State>();
            foreach (var field in s.GetType().GetFields(flags))
            {
                if (!typeof(State).IsAssignableFrom(field.FieldType))
                {
                    continue;
                }
                if (field.Name == "Parent")
                {
                    continue;
                }

                State child = (State)field.GetValue(s);
                if (child == null)
                {
                    continue;
                }
                if (!ReferenceEquals(child.Parent, s))
                {
                    continue;
                }

                children.Add(child);
                Wire(child, m, visited);
            }
            s.SetChildren(children);
        }
    }
}