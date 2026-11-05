using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    Vector2 _direction;
    [SerializeField] float _speed;

    private void Start()
    {
        Destroy(gameObject, 3f);
    }

    private void Update()
    {
        transform.Translate(_direction * _speed * Time.deltaTime);
    }

    public void SetDirection(Vector2 direction)
    {
        _direction = direction;
    }
}
