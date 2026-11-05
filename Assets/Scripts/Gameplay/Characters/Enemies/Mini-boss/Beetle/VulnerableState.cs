using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using Core.Utility;

public class VulnerableState : MonoBehaviour, IBossState
{
    [SerializeField] SpriteRenderer _visualBody;
    [SerializeField] HitBoxCheck _vulnerableHitBox;
    [SerializeField] int _stunTimer;
    HealthSystem _healthSystem;

    float randomTimer;

    private void Start()
    {
        _healthSystem = GetComponent<HealthSystem>();
    }

    public bool Evaluate()
    {
        return false;
    }

    public async void Execute()
    {
        randomTimer = Random.Range(_stunTimer * 0.6f, _stunTimer * 1.4f);
        await ActiveVulnarableState();
    }

    private async UniTask ActiveVulnarableState()
    {
        _visualBody.flipY = true;

        await UniTask.Delay((int)(randomTimer * 1000));

        ExitState();

        await UniTask.Yield();
    }

    public void Tick()
    {
        var hit = _vulnerableHitBox.GetHitBoxHit();
        if (hit)
        {
            _healthSystem.TakeDamage(1);

            var playerRb = hit.GetComponentInParent<Rigidbody2D>();
            GameplayUtilities.OnPlayerKickOutRequest?.Invoke(playerRb, transform, 6, default);

            ExitState();
        }
    }

    private void ExitState()
    {
        _visualBody.flipY = false;

        BossController.OnBaseStateRequest?.Invoke();
    }

    public StateType GetStateType()
    {
        return StateType.Strict;
    }

    
}
