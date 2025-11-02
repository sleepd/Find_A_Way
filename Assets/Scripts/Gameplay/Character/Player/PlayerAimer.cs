using UnityEngine;

/// <summary>
/// Handles converting mouse position into a world-space aim target and rotates the player accordingly.
/// </summary>
public sealed class PlayerAimer
{
    private readonly Transform _playerTransform;
    private readonly Camera _camera;
    private readonly float _rotationSpeed;

    public PlayerAimer(Transform playerTransform, Camera camera, float rotationSpeed)
    {
        _playerTransform = playerTransform;
        _camera = camera;
        _rotationSpeed = rotationSpeed;
    }

    /// <summary>
    /// Raycast from the provided mouse position to the ground and rotate the player to face that point.
    /// </summary>
    public void AimAtScreenPosition(Vector2 screenPosition)
    {
        Ray aimRay = _camera.ScreenPointToRay(screenPosition);

        // Intersect the ray with a horizontal plane at the player's Y level.
        var groundPlane = new Plane(Vector3.up, new Vector3(0f, _playerTransform.position.y, 0f));
        if (groundPlane.Raycast(aimRay, out float enter))
        {
            Vector3 targetPosition = aimRay.GetPoint(enter);
            Vector3 direction = targetPosition - _playerTransform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _playerTransform.rotation = Quaternion.RotateTowards(
                    _playerTransform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }
        }
    }
}
