using Cysharp.Threading.Tasks;
using UnityEngine;
using Core.Utility;

public class RushAttackState : MonoBehaviour, IBossState
{
    [Header("Stat")]
    [SerializeField] float _cdTime;
    [SerializeField] float _moveSpeed;

    [Header("HitBox")]
    [SerializeField] HitBoxCheck _hitBoxCheck;

    [Header("Visual")]
    [SerializeField] Transform _visualBody;
    [SerializeField] SpriteRenderer _sprite;

    [Header("RayCast")]
    [SerializeField] Transform _leftRayCast;
    [SerializeField] Transform _rightRayCast;
    [SerializeField] LayerMask _layerMask;

    Color baseColor;
    float currentCd;
    bool _canAttack = true;

    BossController _bossController;
    PlayerDetector _playerDetector;
    int _playerDirection;
    Rigidbody2D _rb;
    bool wallHit = false;

    //RayCasts
    Vector2 _rayOrigin;
    Vector2 _rayCast;
    bool _isRayLeft = false;

    private void Start()
    {
        _bossController = GetComponent<BossController>();
        _playerDetector = GetComponent<PlayerDetector>();
        _rb = GetComponent<Rigidbody2D>();

        currentCd = _cdTime;
        _canAttack = true;
        baseColor = _sprite.color;
    }

    private void Update()
    {
        if (!_canAttack)
        {
            ReloadAttack();
        }
    }

    public void Tick()
    {
        var player = _hitBoxCheck.GetHitBoxHit();

        if (_isRayLeft)
        {
            _rayOrigin = _leftRayCast.position;
        }
        else
        {
            _rayOrigin = _rightRayCast.position;
        }

        if (player != null)
        {
            HealthSystem hp = player.GetComponent<HealthSystem>();

            hp.TakeDamage(1);
            GameplayUtilities.OnPlayerKickOutRequest?.Invoke(player.GetComponent<Rigidbody2D>(), transform, 6, default);

            _canAttack = false;
            _sprite.color = baseColor;
            BossController.OnBaseStateRequest?.Invoke();
        }

        if (wallHit)
        {
            if (_bossController.VulnarableState != null)
            {
                _canAttack = false;
                _sprite.color = baseColor;
                BossController.OnVulnerableStateRequest?.Invoke();
            }
            else
            {
                _canAttack = false;
                _sprite.color = baseColor;
                BossController.OnBaseStateRequest?.Invoke();
            }
        }

        RayCastActive(_rayOrigin, _rayCast);
    }

    private void ReloadAttack()
    {
        currentCd -= Time.deltaTime;

        if (currentCd <= 0)
        {
            _canAttack = true;
            currentCd = _cdTime;
        }
    }
    public bool Evaluate()
    {
        if (_canAttack)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public async void Execute()
    {
        wallHit = false;
        var playerTarget = _playerDetector.GetPlayer();
        _playerDirection = playerTarget.position.x - transform.position.x > 0 ? 1 : -1;

        SetRay();

        _rb.linearVelocity = Vector2.zero;
        await RushAttack();
    }

    private void SetRay()
    {
        if (_playerDirection < 0)
        {
            _rayOrigin = _leftRayCast.position;
            _rayCast = Vector2.left;
            _isRayLeft = true;
        }

        if (_playerDirection > 0)
        {
            _rayOrigin = _rightRayCast.position;
            _rayCast = Vector2.right;
            _isRayLeft = false;
        }
    }

    private void RayCastActive(Vector2 origin, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, 0.75f, _layerMask);

        if (hit)
        {
            wallHit = true;
        }
    }

    private async UniTask RushAttack()
    {
        _sprite.color = Color.red;

        await UniTask.Delay(700);

        while (!wallHit && !_hitBoxCheck.GetHitBoxHit())
        {
            _rb.AddForce((Vector2.right * _playerDirection) * _moveSpeed * 50, ForceMode2D.Force);
            await UniTask.Delay(50);
        }

        await UniTask.Yield();
    }

    public StateType GetStateType()
    {
        return StateType.Strict;
    }
}
