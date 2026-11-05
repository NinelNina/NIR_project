using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using System.Threading;
using UtilityAI;

namespace HSM
{
    public class TransitionSequencer
    {
        public readonly StateMachine Machine;
        public readonly bool UseSequential = true;

        private ISequence _sequencer;
        private Action _nextPhase;
        private (State from, State to)? _pending;
        private State _lastFrom, _lastTo;
        private CancellationTokenSource _cts;

        public TransitionSequencer(StateMachine machine)
        {
            Machine = machine;
        }

        public void OnBeginTransition(State from, State to)
        {
            if (_sequencer != null)
            {
                _pending = (from, to);
                return;
            }
            BeginTransition(from, to);
        }

        public void Tick(float deltaTime)
        {
            if (_sequencer != null)
            {
                if (_sequencer.Update())
                {
                    if (_nextPhase != null)
                    {
                        var n = _nextPhase;
                        _nextPhase = null;
                        n();
                    }
                    else
                    {
                        EndTransition();
                    }
                }
                return;
            }
            Machine.InternalTick(deltaTime);
        }

        public static State Lca(State a, State b)
        {
            var ap = new HashSet<State>();
            for (State s = a; s != null; s = s.Parent)
            {
                ap.Add(s);
            }

            for (State s = b; s != null; s = s.Parent)
            {
                if (ap.Contains(s))
                {
                    return s;
                }
            }
            return null;
        }

        private static List<PhaseStep> GatherPhaseSteps(List<State> chain, bool deactivate)
        {
            var steps = new List<PhaseStep>();
            for (int i = 0; i < chain.Count; i++)
            {
                var acts = chain[i].Activities;
                for (int j = 0; j < acts.Count; j++)
                {
                    var act = acts[j];

                    if (deactivate && act.Mode == ActivityMode.Active)
                    {
                        steps.Add(ct => act.DeactivateAsync(ct));
                    }
                    else if (act.Mode == ActivityMode.Inactive)
                    {
                        steps.Add(ct => act.ActivateAsync(ct));
                    }
                }
            }

            return steps;
        }

        private static List<State> StatesToExit(State from, State lca)
        {
            var list = new List<State>();
            for (var s = from; s != null && s != lca; s = s.Parent)
            {
                list.Add(s);
            }

            return list;
        }

        private static List<State> StatesToEnter(State to, State lca)
        {
            var stack = new Stack<State>();
            for (var s = to; s != null && s != lca; s = s.Parent)
            {
                stack.Push(s);
            }

            return new List<State>(stack);
        }

        private void ChangeState(State from, State to)
        {
            if (from == to || from == null || to == null)
            {
                return;
            }

            State lca = TransitionSequencer.Lca(from, to);
            for (State s = from; s != lca; s = s.Parent)
            {
                s.Exit();
            }

            var stack = new Stack<State>();
            for (State s = to; s != lca; s = s.Parent)
            {
                stack.Push(s);
            }
            while (stack.Count > 0)
            {
                State newState = stack.Pop();
                if (stack.Count == 0)
                {
                    newState.Enter(true);
                }
                else
                {
                    newState.Enter(false);
                }
            }
        }

        private void BeginTransition(State from, State to)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            var lca = Lca(from, to);
            var exitChain = StatesToExit(from, lca);
            var enterChain = StatesToEnter(to, lca);

            var exitSteps = GatherPhaseSteps(exitChain, deactivate: true);
            //_sequencer = new NoopPhase();
            _sequencer = UseSequential
                ? new SequentialPhase (exitSteps, _cts.Token)
                : new ParallelPhase (exitSteps, _cts.Token);

            _sequencer.Start();

            _nextPhase = () =>
            {
                ChangeState(from, to);
                var enterSteps = GatherPhaseSteps(enterChain, deactivate: false);
                //_sequencer = new NoopPhase();
                _sequencer = UseSequential
                ? new SequentialPhase(enterSteps, _cts.Token)
                : new ParallelPhase(enterSteps, _cts.Token);
                _sequencer.Start();
            };
        }

        private void EndTransition()
        {
            _sequencer = null;

            if (_pending.HasValue)
            {
                (State from, State to) p = _pending.Value;
                _pending = null;
                BeginTransition(p.from, p.to);
            }
        }
    }
}
