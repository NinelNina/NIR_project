using UnityEngine;

public class LandingDamage : MonoBehaviour
{
    [SerializeField] private float _stompImpulse = 2f;
    [SerializeField] private int _damage = 1;

    public int Damage => _damage;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponentInParent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<LandingVulnerability>(out var vulnerability))
        {
            vulnerability.OnGetHit(gameObject, _damage);

            if (_rb != null)
            {
                _rb.linearVelocityY = 0;
                _rb?.AddForce(Vector2.up * _stompImpulse, ForceMode2D.Impulse);
            }
        }
    }
}
