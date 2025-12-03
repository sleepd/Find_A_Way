using UnityEngine;

public class PlayerMovement
{
    private readonly Transform _transform;
    private readonly CharacterController _characterController;
    private readonly float _rotationSpeed;
    private readonly Transform _meshRoot;
    private Vector3 _velocity;
    public Vector3 Velocity => _velocity;

    public PlayerMovement(CharacterController characterController, Animator animator, float rotationSpeed)
    {
        _characterController = characterController;
        _rotationSpeed = rotationSpeed;
        _transform = characterController.transform;
        _meshRoot = animator != null ? animator.transform : null;
    }


    public void Rotate(Vector3 direction)
    {
        var deltaTime = Time.deltaTime;

        if (direction.sqrMagnitude > 0.0001f)
        {
            var targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            _transform.rotation = Quaternion.RotateTowards(
                _transform.rotation,
                targetRotation,
                _rotationSpeed * deltaTime);
        }
    }

    public void Move(Vector3 delta)
    {
        var deltaTime = Time.deltaTime;

        // Use supplied root motion delta for displacement.
        var rootMotion = delta;

        if (!_characterController.isGrounded)
        {
            // Apply a small downward pull so the controller stays grounded.
            rootMotion += Physics.gravity * deltaTime;
        }
        else if (rootMotion.y < 0f)
        {
            rootMotion.y = 0f;
        }

        _characterController.Move(rootMotion);

        if (deltaTime > Mathf.Epsilon)
        {
            _velocity = rootMotion / deltaTime;
        }

        // Reset child mesh so root motion only drives the parent once.
        if (_meshRoot != null)
        {
            _meshRoot.localPosition = Vector3.zero;
            _meshRoot.localRotation = Quaternion.identity;
        }
    }
}
