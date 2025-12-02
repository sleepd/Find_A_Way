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
    [SerializeField] Transform weaponSlot;

    #region components
    public PlayerMovement Movement {get; private set;}
    public PlayerInput Input {get; private set;}
    private PlayerAimer playerAimer;
    public CharacterHealth Health { get; private set; }
    public InventoryManager Inventory { get; private set; }
    public WeaponLoadoutController WeaponLoadoutController { get; private set; }
    private InteractionSensor interactionSensor;
    public Animator AnimatorController {get; private set;}
    public PlayerStateMachin StateMachin {get; private set;}
    #endregion

    [SerializeField] private Weapon[] initializeWeapons; 

    [Header("Interaction")]
    [SerializeField] private LayerMask interactableMask = ~0;
    [SerializeField, Min(0.5f)] private float interactionScanRadius = 3f;

    public IInteractable CurrentInteractable => interactionSensor?.Current;
    public InteractionSensor InteractionSensor => interactionSensor;

    void Awake()
    {
        CharacterController characterController = GetComponent<CharacterController>();
        AnimatorController = GetComponentInChildren<Animator>();
        Movement = new(characterController, acceleration, maxSpeed, drag);
        playerAimer = new(transform, Camera.main, rotationSpeed);
        Input = new();
        Health = new(maxHealth);
        WeaponLoadoutController = new(new WeaponLoadout(2));
        interactionSensor = new InteractionSensor(this, transform, interactionScanRadius, interactableMask);
        StateMachin = new(this);
        StateMachin.Initialize(PlayerStateDictionary.Build(StateMachin), typeof(PlayerStateIdle));

        // temporary add weapons
        for (int i = 0; i < WeaponLoadoutController.Loadout.SlotCount; i++)
        {
            Weapon newWeapon = Instantiate(initializeWeapons[i], weaponSlot);
            WeaponLoadoutController.AssignWeapon(i, newWeapon);
        }
        WeaponLoadoutController.EquipSlot(0);
    }

    void Update()
    {
        Movement.Move(Input.MoveDirection);
        playerAimer.AimAtScreenPosition(Input.PointerPosition);
        if (WeaponLoadoutController.CurrentWeapon != null)
        {
            WeaponLoadoutController.CurrentWeapon.AimAtScreenPosition(Input.PointerPosition);
        }
        interactionSensor?.Tick();
    }

    void OnEnable()
    {
        Input.FireStarted += HandleFireStarted;
        Input.FireCanceled += HandleFireCanceled;
        Input.ReloadTriggered += HandleReload;
        Input.NextWeaponTriggered += HandleNextWeapon;
        Input.PreviousWeaponTriggered += HandlePreviousWeapon;
    }

    void OnDisable()
    {
        Input.FireStarted -= HandleFireStarted;
        Input.FireCanceled -= HandleFireCanceled;
        Input.ReloadTriggered -= HandleReload;
        Input.NextWeaponTriggered -= HandleNextWeapon;
        Input.PreviousWeaponTriggered -= HandlePreviousWeapon;
        Input.Dispose();
    }

    void HandleFireStarted() => WeaponLoadoutController.CurrentWeapon.BeginFire();
    void HandleFireCanceled() => WeaponLoadoutController.CurrentWeapon.EndFire();
    void HandleReload() => WeaponLoadoutController.CurrentWeapon.Reload();

    void HandleNextWeapon() => WeaponLoadoutController.EquipNext(1);
    void HandlePreviousWeapon() => WeaponLoadoutController.EquipNext(-1);
}
