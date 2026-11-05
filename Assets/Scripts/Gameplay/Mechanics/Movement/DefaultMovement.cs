using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DefaultMovement : MonoBehaviour
{
    [SerializeField] private BaseChecker _groundChecker;

    [SerializeField] private float _jumpAcceleration = 3f;
    [SerializeField] private float _horizontalSpeed = 3f;
    [SerializeField] private float _horizontalAcceleration = 10f;
    [SerializeField] private float _jumpCooldownTime = 0.1f;

    [SerializeField] private bool _allowFlipping = true;

    private Rigidbody2D _rb;

    private float _horizontalDirection = 0;

    private bool _canJump = true;
    private bool _isFacingRight = true;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _isFacingRight = true;
    }

    private void FixedUpdate()
    {
        _rb.AddForce(new Vector2(_horizontalDirection * _horizontalAcceleration, 0), ForceMode2D.Force);
        _rb.linearVelocityX = Mathf.Clamp(_rb.linearVelocityX, -_horizontalSpeed, _horizontalSpeed);
        if (_groundChecker != null && _horizontalDirection == 0)
        {
            _rb.linearVelocityX = 0;
        }
    }

    public void SetFacingDirection(bool isFacingRight)
    {
        if (isFacingRight != _isFacingRight)
        {
            Flip();
        }
        _isFacingRight = isFacingRight;
    }

    public void SetDirection(float direction)
    {
        direction = Mathf.Clamp(direction, -1, 1);
        _horizontalDirection = direction;

        if (_allowFlipping && direction != 0)
        {
            SetFacingDirection((direction > 0));
        }
    }

    public void Jump()
    {
        if (_canJump)
        {
            if (_groundChecker && _groundChecker.IsDetecting)
            {
                _rb.AddForce(new Vector2(0, _jumpAcceleration), ForceMode2D.Impulse);
                StartCoroutine(JumpCooldown(_jumpCooldownTime));
            }
        }
    }

    private IEnumerator JumpCooldown(float delay)
    {
        _canJump = false;
        yield return new WaitForSeconds(delay);
        _canJump = true;
    }

    private void Flip()
    {
        transform.rotation *= Quaternion.Euler(0, 180, 0);
    }
}
