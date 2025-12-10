using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lightweight wrapper around the generated InputSystem actions for gameplay input.
/// Provides movement input without coupling to specific movement logic.
/// </summary>
public sealed class PlayerInput : IDisposable
{
    public event Action FireStarted;
    public event Action FireCanceled;
    public event Action AimingStarted;
    public event Action AimingCanceled;
    public event Action ReloadTriggered;
    public event Action NextWeaponTriggered;
    public event Action PreviousWeaponTriggered;
    public event Action InteractTriggered;
    public bool IsFiring { get; private set; }
    public bool IsAiming { get; private set; }
    private readonly InputSystem_Actions _inputActions;

    public PlayerInput()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Player.Attack.performed += OnFirePerformed;
        _inputActions.Player.Attack.canceled += OnFireCanceled;
        _inputActions.Player.Aiming.performed += OnAimPerformed;
        _inputActions.Player.Aiming.canceled += OnAimCanceled;
        _inputActions.Player.Reload.performed += OnReloadPerformed;
        _inputActions.Player.Next.performed += OnNextPerformed;
        _inputActions.Player.Previous.performed += OnPreviousPerformed;
        _inputActions.Player.Interact.performed += OnInteractPerformed;
        Enable();
    }

    /// <summary>
    /// Current move input on the XZ plane (Vector2.x -> world X, Vector2.y -> world Z).
    /// </summary>
    public Vector2 MoveInput => _inputActions.Player.Move.ReadValue<Vector2>();

    /// <summary>
    /// Convenience helper to get the move input as a world-space Vector3 on the horizontal plane.
    /// </summary>
    public Vector3 MoveDirection => new Vector3(MoveInput.x, 0f, MoveInput.y);

    /// <summary>
    /// Current pointing position on the screen (e.g., mouse or gamepad cursor).
    /// </summary>
    public Vector2 PointerPosition => _inputActions.Player.Look.ReadValue<Vector2>();

    void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        IsFiring = true;
        FireStarted?.Invoke();
    }

    void OnFireCanceled(InputAction.CallbackContext ctx)
    {
        IsFiring = false;
        FireCanceled?.Invoke();
    }

    void OnAimPerformed(InputAction.CallbackContext ctx)
    {
        IsAiming = true;
        AimingStarted?.Invoke();
    }

    void OnAimCanceled(InputAction.CallbackContext ctx)
    {
        IsAiming = false;
        AimingCanceled?.Invoke();
    }

    void OnReloadPerformed(InputAction.CallbackContext ctx)
    {
        ReloadTriggered?.Invoke();
    }

    void OnNextPerformed(InputAction.CallbackContext ctx)
    {
        NextWeaponTriggered?.Invoke();
    }

    void OnPreviousPerformed(InputAction.CallbackContext ctx)
    {
        PreviousWeaponTriggered?.Invoke();
    }

    void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        InteractTriggered?.Invoke();
    }

    public void Enable()
    {
        _inputActions.Player.Enable();
    }

    public void Disable()
    {
        _inputActions.Player.Disable();
    }

    public void Dispose()
    {
        Disable();
        _inputActions.Player.Attack.performed -= OnFirePerformed;
        _inputActions.Player.Attack.canceled -= OnFireCanceled;
        _inputActions.Player.Aiming.performed -= OnAimPerformed;
        _inputActions.Player.Aiming.canceled -= OnAimCanceled;
        _inputActions.Player.Reload.performed -= OnReloadPerformed;
        _inputActions.Player.Next.performed -= OnNextPerformed;
        _inputActions.Player.Previous.performed -= OnPreviousPerformed;
        _inputActions.Player.Interact.performed -= OnInteractPerformed;
        _inputActions.Dispose();
    }
}
