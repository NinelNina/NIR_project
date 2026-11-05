using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HSM
{
    public class SequentialPhase : ISequence
    {
        public bool IsDone { get; private set; }

        private readonly List<PhaseStep> _steps;
        private readonly CancellationToken _ct;
        private Task _current;
        private int _index = -1;

        public SequentialPhase(List<PhaseStep> steps, CancellationToken ct)
        {
            _steps = steps;
            _ct = ct;
        }

        public void Start()
        {
            Next();
        }

        public bool Update()
        {
            if (IsDone)
            {
                return true;
            }

            if (_current == null || _current.IsCompleted)
            {
                Next();
            }

            return IsDone;
        }

        private void Next()
        {
            _index++;
            if (_index >= _steps.Count)
            {
                IsDone = true;
                return;
            }
            _current = _steps[_index](_ct);
        }
    }
}
