using Cysharp.Threading.Tasks;
using UnityEngine;
using Core.Utility;

public class StompState : MonoBehaviour, IBossState
{
    [Header("Stat")]
    [SerializeField] float _attackCd;
    [SerializeField] HitBoxCheck _attackPrefab;
    [SerializeField] Transform _attackSpawnPoint;

    [Header("Other")]
    [SerializeField] Transform _visualBody;
    [SerializeField] SpriteRenderer _spriteRenderer;

    HitBoxCheck _currentAttack;
    PlayerDetector _playerDetector;


    Color baseColor;
    bool _canAttack;
    float _currentCd;

    private void Start()
    {
        _playerDetector = GetComponent<PlayerDetector>();
    }

    private void Update()
    {
        if (!_canAttack)
        {
            ReloadAttack();
        }

        if (_currentAttack != null)
        {
            var hit = _currentAttack.GetHitBoxHit();
            if (hit)
            {
                HealthSystem hp = hit.GetComponent<HealthSystem>();

                if (hp != null)
                {
                    hp.TakeDamage(1);

                    GameplayUtilities.OnPlayerKickOutRequest?.Invoke(hp.GetComponent<Rigidbody2D>(), _currentAttack.transform, 6, default);

                    Destroy(_currentAttack.gameObject);
                    _currentAttack = null;
                }
            }
        }
    }

    private void ReloadAttack()
    {
        _currentCd -= Time.deltaTime;

        if (_currentCd <= 0)
        {
            _canAttack = true;
            _currentCd = _attackCd;
        }
    }

    public async void Execute()
    {
        baseColor = _spriteRenderer.color;

        _spriteRenderer.color = Color.red;
        await StompAttack();
    }

    private async UniTask StompAttack()
    {
        _visualBody.Rotate(0, _visualBody.rotation.y, 45);

        await UniTask.Delay(1500);

        _visualBody.Rotate(0, _visualBody.rotation.y, -45);

        var newAttack = Instantiate(_attackPrefab, _attackSpawnPoint.position, _attackPrefab.transform.rotation);
        _currentAttack = newAttack;
        _currentAttack.GetComponent<EnemyProjectile>().SetDirection(PlayerDirection());

        await UniTask.Delay(500);
        _canAttack = false;
        _spriteRenderer.color = baseColor;

        BossController.OnBaseStateRequest?.Invoke();

        await UniTask.Yield();
    }

    private Vector2 PlayerDirection()
    {
        var player = _playerDetector.GetPlayer();

        int direction = player.transform.position.x - transform.position.x > 0 ? 1 : -1;

        return Vector2.right * direction;
    }

    public void Tick()
    {
        
    }

    public bool Evaluate()
    {
        if (_canAttack)
        {
            _currentAttack = null;
            return true;
        }
        else
        {
            return false;
        }
    }

    public StateType GetStateType()
    {
        return StateType.Strict;
    }

    
}
