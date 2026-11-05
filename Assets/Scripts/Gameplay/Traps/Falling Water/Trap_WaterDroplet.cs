using UnityEngine;
using Cysharp.Threading.Tasks;
using Core.Utility;

public class Trap_WaterDroplet : MonoBehaviour, ITrap
{
    [SerializeField] float _kickOutForce;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var hp = collision.gameObject.GetComponent<HealthSystem>();

        if (hp != null)
        {
            var rb = hp.GetComponent<Rigidbody2D>();
            ActivateTrap(hp, rb);
        }

        //Отключаем визуал и коллизии у капли
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        sprite.color = new Color(0, 0, 0, 0);
        collider.enabled = false;

        //Удаляем каплю в любом случае (можно потом сделать через object pooling, пока так)
        Destroy(gameObject, 0.4f);
    }

    public void ActivateTrap(HealthSystem health = default, Rigidbody2D rb = default, Transform teleportPoint = null)
    {
        //Отключаем управление игрока
        GameEventsManager.OnPlayerControlsOff?.Invoke();

        //Наносим урон
        health.TakeDamage(1);

        //Отбрасываем игрока от ловушки
        GameplayUtilities.OnPlayerKickOutRequest?.Invoke(rb, transform, _kickOutForce, default);
    }
}
