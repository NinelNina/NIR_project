using System;
using System.Threading;
using System.Threading.Tasks;

namespace HSM
{
    public class DelayActivity : Activity
    {
        public float Seconds = 0.2f;

        public override async Task ActivateAsync(CancellationToken ct)
        {
            await Task.Delay(TimeSpan.FromSeconds(Seconds), ct);
            await base.ActivateAsync(ct);
        }
    }
}
