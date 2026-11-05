using System.Numerics;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private PlayerControls _playerControls;

    private void Awake()
    {
        _playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        _playerControls.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

    public float GetPlayerMovement()
    {
        return _playerControls.Player.Move.ReadValue<float>();
    }

    public bool GetPlayerSprint()
    {
        return _playerControls.Player.Sprint.triggered;
    }

    public bool GetPlayerJump()
    {
        return _playerControls.Player.Jump.triggered;
    }

    public bool GetPlayerJumpHeld()
    {
        return _playerControls.Player.Jump.IsInProgress();
    }

    public bool GetPlayerDuck()
    {
        return _playerControls.Player.Duck.triggered;
    }
}
