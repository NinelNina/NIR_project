using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PassablePlatform : MonoBehaviour
{
    private const float DefaultPassDuration = 0.7f;
    private const string PlayerLayerName = "Player";

    [SerializeField] private PlatformEffector2D _effector;
    [SerializeField] private LayerMask _baseLayerMask = ~0;
    [SerializeField] private LayerMask _playerPassLayerMask = ~0;
    [SerializeField, Min(0f)] private float _passDuration = DefaultPassDuration;

    private CancellationTokenSource _pass;

    public bool IsPassing => _pass != null;

    public void Configure(PlatformEffector2D effector)
    {
        _effector = effector;
        _baseLayerMask = ~0;
        _playerPassLayerMask = WithoutPlayer();
    }

    public void ActivateTrap() => PassAsync(this.GetCancellationTokenOnDestroy()).Forget();

    public async UniTask PassAsync(CancellationToken ct = default)
    {
        if (_effector == null)
            return;

        _pass?.Cancel();
        _pass?.Dispose();
        var pass = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _pass = pass;
        _effector.colliderMask = _playerPassLayerMask;

        bool cancelled = await UniTask.Delay(TimeSpan.FromSeconds(_passDuration), cancellationToken: pass.Token).SuppressCancellationThrow();

        if (_pass != pass)
            return;

        _pass = null;
        pass.Dispose();

        if (!cancelled && _effector != null)
            _effector.colliderMask = _baseLayerMask;
    }

    private void Awake()
    {
        if (_effector == null)
            TryGetComponent(out _effector);
    }

    private void Reset()
    {
        TryGetComponent(out _effector);
        _playerPassLayerMask = WithoutPlayer();
    }

    private void OnDestroy()
    {
        _pass?.Cancel();
        _pass?.Dispose();
        _pass = null;
    }

    private static LayerMask WithoutPlayer()
    {
        int player = LayerMask.NameToLayer(PlayerLayerName);
        return player >= 0 ? ~(1 << player) : ~0;
    }
}
