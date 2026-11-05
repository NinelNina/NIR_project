using UnityEngine;

namespace Level.Platforms
{
    [CreateAssetMenu(fileName = "SurfaceProfile", menuName = "Level/Surface Profile")]
    public sealed class SurfaceProfile : ScriptableObject
    {
        [Header("Движение")]
        [SerializeField, Min(0f)] private float _acceleration = 1f;
        [SerializeField, Min(0f)] private float _deceleration = 1f;
        [SerializeField, Min(0f)] private float _maxSpeed = 1f;
        [SerializeField, Min(0f)] private float _jump = 1f;

        [Header("Шаги")]
        [SerializeField] private AudioClip[] _footsteps = new AudioClip[0];
        [SerializeField] private ParticleSystem _stepParticles;

        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float MaxSpeed => _maxSpeed;
        public float Jump => _jump;
        public AudioClip[] Footsteps => _footsteps;
        public ParticleSystem StepParticles => _stepParticles;
    }
}
