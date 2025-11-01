using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lightweight wrapper around the generated InputSystem actions for gameplay input.
/// Provides movement input without coupling to specific movement logic.
/// </summary>
public sealed class PlayerInput : IDisposable
{
    private readonly InputSystem_Actions _inputActions;

    public PlayerInput()
    {
        _inputActions = new InputSystem_Actions();
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
        _inputActions.Dispose();
    }
}
