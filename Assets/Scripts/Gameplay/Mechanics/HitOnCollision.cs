using UnityEngine;

public class HitOnCollision : MonoBehaviour
{
    [SerializeField] private int _damage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<HealthSystem>(out var otherHealth))
        {
            otherHealth.TakeDamage(_damage);
        }
        Destroy(gameObject);
    }
}
