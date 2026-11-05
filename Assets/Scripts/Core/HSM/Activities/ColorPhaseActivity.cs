using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HSM
{
    public class ColorPhaseActivity : Activity
    {
        public Color enterColor = Color.red;
        public Color exitColor = Color.yellow;

        private readonly Renderer _renderer;
        private readonly Material _mat;

        public ColorPhaseActivity(Renderer r)
        {
            _renderer = r;
            if (_renderer != null)
            {
                _mat = _renderer.material;
            }
        }

        public override async Task ActivateAsync(CancellationToken ct)
        {
            if (Mode != ActivityMode.Inactive || _mat == null)
            {
                return;
            }
            Mode = ActivityMode.Activating;
            _mat.color = enterColor;
            Mode = ActivityMode.Active;
        }

        public override async Task DeactivateAsync(CancellationToken ct)
        {
            if (Mode != ActivityMode.Active || _mat == null)
            {
                return;
            }
            Mode = ActivityMode.Deactivating;
            _mat.color = exitColor;
            Mode = ActivityMode.Inactive;
        }
    }
}