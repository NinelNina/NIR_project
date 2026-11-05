using System;
using UnityEngine;

public class PlayerLedgeCheck : MonoBehaviour
{
    public Action<bool> OnLedgeDetectAbility;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            OnLedgeDetectAbility?.Invoke(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            OnLedgeDetectAbility?.Invoke(true);
        }
    }
}
