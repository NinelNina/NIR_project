using UnityEngine;

public class Healing_Platform : MonoBehaviour, ITrap
{
    [SerializeField] float _timeBetweenHeal;
    private float _currentTimer;
    private bool _canHeal;

    HealthSystem player;

    private void Start()
    {
        _currentTimer = _timeBetweenHeal;
    }

    private void Update()
    {
        if (_canHeal)
        {
            _currentTimer -= Time.deltaTime;
            if (_currentTimer <= 0)
            {
                ActivateTrap(player);
                _currentTimer = _timeBetweenHeal;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var health = collision.gameObject.GetComponent<HealthSystem>();

        if (health != null)
        {
            player = health;
            _canHeal = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _currentTimer = _timeBetweenHeal;
        _canHeal = false;
        player = null;
    }

    public void ActivateTrap(HealthSystem health = null, Rigidbody2D rb = null, Transform teleportPoint = null)
    {
        health.TakeDamage(-1);
    }
}
