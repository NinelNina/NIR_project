using System;
using UnityEngine;

public class PlayerStealthCheck : MonoBehaviour
{
    [SerializeField] LayerMask _layerMaskForStealth;
    [SerializeField] float _radius;
    public Action<bool> OnStealthChange;

    private void FixedUpdate()
    {
        if (Physics2D.OverlapCircle(transform.position, _radius, _layerMaskForStealth))
        {
            OnStealthChange?.Invoke(true);
        }
        else
        {
            OnStealthChange?.Invoke(false);
        }
    }
}
