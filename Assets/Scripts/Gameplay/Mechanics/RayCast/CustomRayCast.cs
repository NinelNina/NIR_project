using Cysharp.Threading.Tasks.Triggers;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomRayCast : MonoBehaviour
{
    [SerializeField] private Transform _point;

    [SerializeField] private LayerMask _layerMask;

    [SerializeField] private Vector2 Direction;

    [SerializeField, Min(float.Epsilon)] public float Length = 1;

    [Header("Gizmos")]
    [SerializeField] private Color _gizmosColor = Color.white;

    public void SetMask(LayerMask newLayerMask)
    {
        _layerMask = newLayerMask;
    }

    public bool CheckCollision(out Transform collisionPoint)
    {
        collisionPoint = null;

        var normalizedDir = transform.localToWorldMatrix * Direction.normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, normalizedDir, Length,  _layerMask);
        if (hit)
        {
            collisionPoint = hit.transform;
            return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        if (_point == null)
        {
            return;
        }
        Debug.DrawRay(_point.position, transform.localToWorldMatrix * Direction.normalized * Length, _gizmosColor);
    }
}
