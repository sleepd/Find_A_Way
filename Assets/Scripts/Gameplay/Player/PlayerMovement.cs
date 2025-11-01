using UnityEngine;

public class PlayerMovement
{
    private CharacterController _characterController;
    private Vector3 _velocity;
    public Vector3 Velocity => _velocity;
    private float _drag;
    private float _acceleration;
    private float _maxSpeed;
    
    public PlayerMovement(CharacterController characterController, float acceleration, float maxSpeed, float drag)
    {
        _characterController = characterController;
        _acceleration = acceleration;
        _maxSpeed = maxSpeed;
        _drag = drag;
    }

    public void Move(Vector3 direction)
    {
        var deltaTime = Time.deltaTime;

        if (direction.sqrMagnitude > 0f)
        {
            direction = direction.normalized;
            _velocity += direction * (_acceleration * deltaTime);

            // Gradually remove any velocity that does not align with the new input direction.
            var alongInput = Vector3.Project(_velocity, direction);
            var perpendicular = _velocity - alongInput;
            perpendicular = Vector3.MoveTowards(perpendicular, Vector3.zero, _drag * deltaTime);
            _velocity = alongInput + perpendicular;
        }
        else
        {
            // No input: apply drag so the player slows to a stop.
            _velocity = Vector3.MoveTowards(_velocity, Vector3.zero, _drag * deltaTime);
        }

        if (_velocity.magnitude > _maxSpeed)
        {
            // Cap movement speed so we never exceed the configured maximum.
            _velocity = _velocity.normalized * _maxSpeed;
        }

        // Convert velocity (units/sec) into a per-frame displacement.
        _characterController.Move(_velocity * deltaTime);
    }
}
