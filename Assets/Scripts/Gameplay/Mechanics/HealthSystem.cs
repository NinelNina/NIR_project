using System;
using UnityEngine;
using Core.Utility;
using Reflex.Attributes;
using EntityType = GameEventsManager.EntityType;

public class HealthSystem : MonoBehaviour
{
    [Inject] GameplayUtilities _utilities;

    [Header("Base Stats")]
    [SerializeField] private int _maxHp;
    [SerializeField, Range(0f, 1.5f)] private float _invulTime = 0.5f; //время неуязвимости после получения урона
    private int _currentHp;

    [Header("Entity Type")]
    [SerializeField] EntityType _entityType = EntityType.None;

    private void Start()
    {
        if (GetComponent<PlayerController>() != null)
        {
            _entityType = EntityType.Player;
        }
        else if (GetComponent<BossController>() != null)
        {
            _entityType = EntityType.Boss;
        }
        else
        {
            _entityType = EntityType.BaseEnemy;
        }

        _currentHp = _maxHp;

        GameEventsManager.OnEntityDeath += OnDeath;

        if (_entityType == EntityType.Player)
        {
            GameEventsManager.OnHealthChanged?.Invoke(_currentHp);
        }

        if (_entityType == EntityType.Boss)
        {
            GameEventsManager.OnBossHealthChanged?.Invoke(_currentHp);
        }
    }

    public void SetMaxHp(int hp)
    {
        _maxHp = hp;
        _currentHp = _maxHp;

        if (_entityType == EntityType.Boss)
        {
            GameEventsManager.OnBossHealthChanged?.Invoke(_currentHp);
        }
    }

    //Урон может быть также и отрицательным для лечения в этом методе
    public void TakeDamage(int damage)
    {
        //Если объект НЕ находится в неуязвимости, то он может получить урон
        if (_utilities.GetInvulnarableState() == false)
        {
            //Наносим урон
            _currentHp = Mathf.Clamp(_currentHp - damage, 0, _maxHp);

            //Обновляем интерфейс для игрока или босса
            if (_entityType == EntityType.Player)
            {
                GameEventsManager.OnHealthChanged?.Invoke(_currentHp);
            }

            if (_entityType == EntityType.Boss)
            {
                GameEventsManager.OnBossHealthChanged?.Invoke(_currentHp);
            }

            //Проверяем оставшееся хп - убиваем объект или включаем неуязвимость
            if (_currentHp <= 0)
            {
                GameEventsManager.OnEntityDeath?.Invoke(_entityType);
            }
            else
            {
                _utilities.SetInvulnarableState(_invulTime);
            }
        }   
    }

    private void OnDeath(EntityType entityType)
    {
        if (_currentHp <= 0)
        {
            if (entityType == EntityType.Player)
            {
                PlayerDeath();
            }
            else if (entityType == EntityType.Boss)
            {
                BossKilled();
            }
            else
            {
                EnemyKilled();
            }
        }
    }

    private void PlayerDeath()
    {
        //пока ничего
        var camera = FindAnyObjectByType<Camera>();
        camera.gameObject.transform.SetParent(null); // вытаскиваем камеру, если она была к чему-то привязана

        Destroy(gameObject);
    }

    private void EnemyKilled()
    {
        //пока ничего
        Destroy(gameObject);
    }

    private void BossKilled()
    {
        //пока ничего
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        GameEventsManager.OnEntityDeath -= OnDeath;
    }
}
