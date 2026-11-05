using UnityEngine;
using UnityEngine.Events;

public class GroundColliderChecker : BaseChecker
{
    [SerializeField] private LayerMask _groundLayer;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsInLayerMask(other.gameObject.layer, _groundLayer))
            IsDetecting = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsInLayerMask(other.gameObject.layer, _groundLayer))
            IsDetecting = false;
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
