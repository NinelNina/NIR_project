using UnityEngine;
using System.Collections;

public class SimplePatrollingEnemy : BaseEnemy
{
    [Header("Patrolling settings")]
    [SerializeField] private Transform _pointA;
    [SerializeField] private Transform _pointB;
    [SerializeField] private float _waitTimeAtPoints = 1f;
    [SerializeField] private bool _startMovingRight = true;
    
    private Vector2 _currentTarget;
    private bool _isWaiting = false;
    
    protected void Start()
    {
        if (_pointA == null || _pointB == null)
        {
            Debug.LogError("Patrol points does not appointed", gameObject);
            return;
        }
        
        _currentTarget = _startMovingRight ? _pointB.position : _pointA.position;
        
        if ((_currentTarget.x > transform.position.x && !_isFacingRight) || (_currentTarget.x < transform.position.x && _isFacingRight))
        {
            Flip();
        }
    }
    
    protected override void Update()
    {
        if (_isDead) return;
        
        Patrol();
    }
    
    protected override void Move()
    {
        if (_isWaiting) return;
        
        float direction = _currentTarget.x > transform.position.x ? 1 : -1;
        _rb.linearVelocity = new Vector2(direction * _moveSpeed, _rb.linearVelocity.y);
        
        if ((direction > 0 && !_isFacingRight) || (direction < 0 && _isFacingRight))
        {
            Flip();
        }
        
        if (Mathf.Abs(transform.position.x - _currentTarget.x) < 0.1f)
        {
            StartCoroutine(WaitAtPoint());
        }
    }
    
    private void Patrol()
    {
        Move();
    }
    
    private IEnumerator WaitAtPoint()
    {
        _isWaiting = true;
        _rb.linearVelocity = Vector2.zero;
        
        yield return new WaitForSeconds(_waitTimeAtPoints);
        
        _currentTarget = _currentTarget == (Vector2)_pointA.position ? _pointB.position : _pointA.position;
        
        _isWaiting = false;
    }
    
    public override void Attack()
    {
        Debug.Log("Attack");
    }
}