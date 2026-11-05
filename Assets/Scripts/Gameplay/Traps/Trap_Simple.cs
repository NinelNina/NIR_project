using Cysharp.Threading.Tasks;
using UnityEngine;
using Reflex.Attributes;
using Core.Utility;

public class Trap_Simple : MonoBehaviour, ITrap
{
    [SerializeField] int _damage;
    [SerializeField] Transform _teleportPoint;
    [SerializeField] float _kickOutForce;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var health = collision.gameObject.GetComponent<HealthSystem>();
        var rb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (health != null && rb != null)
        {
            if (_teleportPoint != null)
            {
                ActivateTrap(health, rb, _teleportPoint);
            }
            else
            {
                ActivateTrap(health, rb);
            }
        }
    }

    public void ActivateTrap(HealthSystem health, Rigidbody2D rb, Transform teleportPoint = default)
    {
        //Отключаем управление игрока
        GameEventsManager.OnPlayerControlsOff?.Invoke();

        //Наносим урон
        health.TakeDamage(_damage);

        GameplayUtilities.OnPlayerKickOutRequest?.Invoke(rb, transform, _kickOutForce, teleportPoint);
    }
}
