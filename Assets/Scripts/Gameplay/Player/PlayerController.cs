using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField, Tooltip("How quickly velocity decays when there is no directional input.")]
    private float _drag;
    [SerializeField, Tooltip("How fast the player accelerates when a direction is pressed.")]
    private float _acceleration;
    [SerializeField, Tooltip("Maximum horizontal speed the player can reach while moving.")]
    private float _maxSpeed;
    [Header("Aim")]
    [SerializeField, Tooltip("Degrees per second the player can rotate toward the aim target.")]
    private float _rotationSpeed = 360f;
    PlayerMovement _playerMovement;
    PlayerInput _playerInput;
    PlayerAimer _playerAimer;

    void Awake()
    {
        CharacterController characterController = GetComponent<CharacterController>();
        _playerMovement = new(characterController, _acceleration, _maxSpeed, _drag);
        _playerAimer = new(transform, Camera.main, _rotationSpeed);
        _playerInput = new();
    }

    void Update()
    {
        _playerMovement.Move(_playerInput.MoveDirection);
        _playerAimer.AimAtScreenPosition(_playerInput.PointerPosition);
    }

    void OnDisable()
    {
        _playerInput.Dispose();
    }
}
