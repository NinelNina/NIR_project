using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HSM
{
    public class ParallelPhase : ISequence
    {
        public bool IsDone { get; private set; }

        private readonly List<PhaseStep> _steps;
        private List<Task> _tasks;
        private readonly CancellationToken _ct;

        public ParallelPhase(List<PhaseStep> steps, CancellationToken ct)
        {
            _steps = steps;
            _ct = ct;
        }

        public void Start()
        {
            if (_steps == null || _steps.Count == 0)
            {
                IsDone = true;
                return;
            }

            _tasks  = new List<Task>(_steps.Count);
            for (int i = 0; i < _steps.Count; i++)
            {
                _tasks.Add(_steps[i](_ct));
            }
        }

        public bool Update()
        {
            if (IsDone)
            {
                return true;
            }

            IsDone = (_tasks == null || _tasks.TrueForAll(t => t.IsCompleted));
            return IsDone;
        }
    }
}
