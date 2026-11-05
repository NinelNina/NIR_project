using UnityEngine;

namespace Level.Platforms
{
    [DisallowMultipleComponent]
    public sealed class GroundSurface : MonoBehaviour
    {
        [SerializeField] private SurfaceProfile _profile;

        public SurfaceProfile Profile => _profile;

        public void SetProfile(SurfaceProfile profile) => _profile = profile;
    }
}
