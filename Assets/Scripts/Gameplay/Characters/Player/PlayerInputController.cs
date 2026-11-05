using Characters.Player;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private PlayerContextHandler _contextHandler;

    public PlayerContext Context => (PlayerContext)_contextHandler.Context;

    public void OnPlayerMove(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            Context.HorizontalDirection = 0;
            return;
        }

        float value = context.ReadValue<float>();
        Context.HorizontalDirection = value;
    }

    public void OnPlayerJump(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            Context.IsJumpPressed = false;
        }
        else if (context.performed)
        {
            Context.IsJumpPressed = true;
        }
    }
}
