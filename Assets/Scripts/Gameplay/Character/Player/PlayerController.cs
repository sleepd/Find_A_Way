using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField, Tooltip("How quickly velocity decays when there is no directional input.")]
    private float drag;
    [SerializeField, Tooltip("How fast the player accelerates when a direction is pressed.")]
    private float acceleration;
    [SerializeField, Tooltip("Maximum horizontal speed the player can reach while moving.")]
    private float maxSpeed;
    [Header("Aim")]
    [SerializeField, Tooltip("Degrees per second the player can rotate toward the aim target.")]
     private float rotationSpeed = 360f;
    [Header("Health")]
    [SerializeField, Tooltip("Max health point")]
    private int maxHealth;

   
    private PlayerMovement playerMovement;
    private PlayerInput playerInput;
    private PlayerAimer playerAimer;
    public CharacterHealth Health { get; private set; }
    [SerializeField] private Weapon currentWeapon; // temporary hard code here
    public Weapon CurrentWeapon => currentWeapon;

    void Awake()
    {
        CharacterController characterController = GetComponent<CharacterController>();
        playerMovement = new(characterController, acceleration, maxSpeed, drag);
        playerAimer = new(transform, Camera.main, rotationSpeed);
        playerInput = new();
        Health = new(maxHealth);
        EquipWeapon();
    }

    void EquipWeapon()
    {
        currentWeapon.SetPlayer(this);
    }

    void Update()
    {
        playerMovement.Move(playerInput.MoveDirection);
        playerAimer.AimAtScreenPosition(playerInput.PointerPosition);
        if (currentWeapon != null)
        {
            currentWeapon.AimAtScreenPosition(playerInput.PointerPosition);
        }
    }

    void OnEnable()
    {
        playerInput.FireStarted += HandleFireStarted;
        playerInput.FireCanceled += HandleFireCanceled;
        playerInput.ReloadTriggered += HandleReload;

    }

    void OnDisable()
    {
        playerInput.FireStarted -= HandleFireStarted;
        playerInput.FireCanceled -= HandleFireCanceled;
        playerInput.ReloadTriggered -= HandleReload;
        playerInput.Dispose();
    }

    void HandleFireStarted() => currentWeapon.BeginFire();
    void HandleFireCanceled() => currentWeapon.EndFire();
    void HandleReload() => currentWeapon.Reload();
}
