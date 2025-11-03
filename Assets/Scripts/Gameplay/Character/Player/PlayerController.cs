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

    #region components
    private PlayerMovement playerMovement;
    private PlayerInput playerInput;
    private PlayerAimer playerAimer;
    public CharacterHealth Health { get; private set; }
    public InventoryManager Inventory { get; private set; }
    public WeaponController WeaponLoadoutController { get; private set; }
    private InteractionSensor interactionSensor;
    #endregion

    [SerializeField] private Weapon currentWeapon; // temporary hard code here
    public Weapon CurrentWeapon => currentWeapon;
    [Header("Interaction")]
    [SerializeField] private LayerMask interactableMask = ~0;
    [SerializeField, Min(0.5f)] private float interactionScanRadius = 3f;

    public IInteractable CurrentInteractable => interactionSensor?.Current;
    public InteractionSensor InteractionSensor => interactionSensor;

    void Awake()
    {
        CharacterController characterController = GetComponent<CharacterController>();
        playerMovement = new(characterController, acceleration, maxSpeed, drag);
        playerAimer = new(transform, Camera.main, rotationSpeed);
        playerInput = new();
        Health = new(maxHealth);
        WeaponLoadoutController = new(new WeaponLoadout(2));
        interactionSensor = new InteractionSensor(this, transform, interactionScanRadius, interactableMask);
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
        interactionSensor?.Tick();
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
