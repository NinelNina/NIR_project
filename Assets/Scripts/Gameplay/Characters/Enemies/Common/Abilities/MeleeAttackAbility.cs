using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeAttackAbility : MonoBehaviour
{
    [SerializeField] private HitBoxCheck _hitBoxCheck;

    [SerializeField] private int _damage = 1;

    [SerializeField] private float _cooldown = 2f;

    private bool _readyToAttack = true;

    public bool CanAttack()
    {
        return (_readyToAttack && _hitBoxCheck.GetHitBoxHit() != null);
    }

    public void TryPerform()
    {
        if (CanAttack())
        {
            Attack();
            StartCoroutine(Cooldown(_cooldown));
        }
    }

    private void Attack()
    {
        bool hasHP = _hitBoxCheck.GetHitBoxHit().TryGetComponent<HealthSystem>(out var otherHP);
        if (hasHP)
        {
            otherHP.TakeDamage(_damage);
        }
    }

    private IEnumerator Cooldown(float delay)
    {
        _readyToAttack = false;
        yield return new WaitForSeconds(delay);
        _readyToAttack = true;
    }
}
