using System.Collections;
using UnityEngine;

public class ShootAbility : MonoBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;

    [SerializeField] private Transform _shootingPoint;

    [SerializeField] private float _shootingForce = 10f;
    [SerializeField] private float _shootingDelay = 1f;
    [SerializeField] private float _maxDistance = 5f;

    private bool _readyToShoot = true;

    public void TryShoot(GameObject target)
    {
        if (_readyToShoot)
        {
            Vector2 direction = CalculateDirection(target);
            Shoot(direction);
            StartCoroutine(Cooldown(_shootingDelay));
        }
    }

    public bool CanHit(GameObject target)
    {
        return (Vector2.Distance(transform.position, target.transform.position) <= _maxDistance);
    }

    private Vector2 CalculateDirection(GameObject target)
    {
        return target.transform.position - _shootingPoint.position;
    }

    private void Shoot(Vector2 direction)
    {
        direction = direction.normalized;

        GameObject projectile = Instantiate(_projectilePrefab, _shootingPoint.position, Quaternion.identity);
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();

        projectileRb.AddForce(direction * _shootingForce, ForceMode2D.Impulse);
    }

    private IEnumerator Cooldown(float delay)
    {
        _readyToShoot = false;
        yield return new WaitForSeconds(delay);
        _readyToShoot = true;
    }
}
