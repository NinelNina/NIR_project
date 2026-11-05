using UnityEngine;

public class MovingPlatform : MonoBehaviour, ITrap
{
    [Header("Параметры движения")]
    [SerializeField] Vector2 direction;
    [SerializeField] float speed;
    [SerializeField] float moveTimer;

    [Header("Параметры Ожидания")]
    [SerializeField] float waitTimer;
    [SerializeField] bool isAlwaysActive;

    [Header("Возврат на начальную точку")]
    [SerializeField] bool isReturning;

    //таймеры
    private float currentTime;
    private float currentWaitTime;

    //Стартовое положение (для некоторых платформ)
    private bool isInStartState = true;

    //Триггеры
    private bool isCurrentlyActive;
    private bool isWaiting;

    private void Start()
    {
        if (isAlwaysActive)
        {
            isCurrentlyActive = true;
        }
    }

    private void Update()
    {
        if (isCurrentlyActive)
        {
            ActivateTrap();

            currentTime += Time.deltaTime;

            if (currentTime >= moveTimer)
            {
                currentTime = 0;
                direction = direction * -1;
                isInStartState = !isInStartState;
                isWaiting = true;
                isCurrentlyActive = false;
            }
        }

        if (isWaiting)
        {
            currentWaitTime += Time.deltaTime;

            if (currentWaitTime >= waitTimer)
            {
                currentWaitTime = 0;
                isWaiting = false;

                //Если платформа всегда активна или если она не находится в стартовом положении 
                //и должна возвращаться (isReturning), то происходит активация
                if (isAlwaysActive || (!isAlwaysActive && (!isInStartState && isReturning)))
                {
                    isCurrentlyActive = true;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.transform.SetParent(transform);

        if (!isAlwaysActive && !isWaiting)
        {
            isCurrentlyActive = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collision.transform.SetParent(null);
    }

    public void ActivateTrap(HealthSystem health = null, Rigidbody2D rb = null, Transform teleportPoint = null)
    {
        if (isCurrentlyActive)
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }
}
