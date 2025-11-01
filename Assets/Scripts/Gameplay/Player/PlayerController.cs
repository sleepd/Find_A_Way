using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField, Tooltip("How quickly velocity decays when there is no directional input.")] private float _drag;
    [SerializeField, Tooltip("How fast the player accelerates when a direction is pressed.")] private float _acceleration;
    [SerializeField, Tooltip("Maximum horizontal speed the player can reach while moving.")] private float _maxSpeed;
    PlayerMovement _playerMovement;
    PlayerInput _playerInput;

    void Awake()
    {
        CharacterController characterController = GetComponent<CharacterController>();
        _playerMovement = new(characterController, _acceleration, _maxSpeed, _drag);
        _playerInput = new();
    }

    void Update()
    {
        _playerMovement.Move(_playerInput.MoveDirection);
    }

    void OnDisable()
    {
        _playerInput.Dispose();
    }
}