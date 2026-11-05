using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HSM
{
    public interface ISequence
    {
        public bool IsDone { get; }

        public void Start();
        public bool Update();
    }

    public delegate Task PhaseStep(CancellationToken ct);

    public class NoopPhase : ISequence
    {
        public bool IsDone { get; private set; }

        public void Start()
        {
            IsDone = true;
        }

        public bool Update()
        {
            return IsDone;
        }
    }
}
