using UnityEngine;


public interface IEnemy
{
    void Attack();
    void Die();
    void Flip();
    bool IsDead { get; }
}
