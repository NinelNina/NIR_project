using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour, IEnemy
{
    [Header("Main characteristics")] 
    [SerializeField] protected float _damage = 10f;
    [SerializeField] protected float _moveSpeed = 3f;
    
    protected Rigidbody2D _rb;
    protected bool _isDead;    
    protected bool _isFacingRight = true;
    
    public bool IsDead => _isDead;
    
    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        if (_rb == null)
            Debug.LogError($"Rigidbody2D not found in {gameObject.name}!", gameObject);
    }
        
    protected abstract void Move();
    
    protected virtual void Update()
    {
        if (_isDead) return;
        Move();
    }

    public abstract void Attack();

    public virtual void Die()
    {
        _isDead = true;
        _rb.linearVelocity = Vector2.zero;
        Destroy(gameObject, 2f);
    }
    
    public virtual void Flip()
    {
        _isFacingRight = !_isFacingRight;
        transform.Rotate(0f, 180f, 0f);
        Debug.Log("Flipped");
    }
}
