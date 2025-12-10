using UnityEngine;

public class PlayerController : MonoBehaviour, IRootMotionParent
{
    [Header("Movement")]
    [SerializeField, Tooltip("Degrees per second the player turns toward movement input.")]
    private float rotationSpeed = 360f;
    [SerializeField, Tooltip("Player move speed while aiming")]
    private float walkingSpeed = 1f;
    [Header("Health")]
    [SerializeField, Tooltip("Max health point")]
    private int maxHealth;
    [SerializeField] Transform weaponSlot;

    #region components
    public PlayerMovement Movement {get; private set;}
    public PlayerInput Input {get; private set;}
    public CharacterHealth Health { get; private set; }
    public InventoryManager Inventory { get; private set; }
    public WeaponLoadoutController WeaponLoadoutController { get; private set; }
    private InteractionSensor interactionSensor;
    public Animator AnimatorController {get; private set;}
    public PlayerStateMachin StateMachin {get; private set;}
    public float RotateSpeed => rotationSpeed;
    public float WalkingSpeed => walkingSpeed;
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
        // We apply root motion ourselves through the CharacterController to keep collision handling consistent.
        // AnimatorController.applyRootMotion = false;
        Movement = new(characterController, AnimatorController, rotationSpeed);
        Input = new();
        Health = new(maxHealth);
        Inventory = new(25);
        WeaponLoadoutController = new(new WeaponLoadout(3));
        interactionSensor = new InteractionSensor(this, transform, interactionScanRadius, interactableMask);
        StateMachin = new(this);
        StateMachin.Initialize(PlayerStateDictionary.Build(StateMachin), typeof(PlayerStateIdle));
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterPlayer(this);
        }

        // temporary add weapons
        for (int i = 0; i < WeaponLoadoutController.Loadout.SlotCount; i++)
        {
            Weapon newWeapon = Instantiate(initializeWeapons[i], weaponSlot);
            newWeapon.SetPlayer(this);
            WeaponLoadoutController.AssignWeapon(i, newWeapon);
        }
        WeaponLoadoutController.EquipSlot(0);
        SetAnimatorWeaponType(WeaponLoadoutController.CurrentWeapon);
    }

    void Update()
    {
        // Movement.Move(Input.MoveDirection);
        StateMachin.Update();
        interactionSensor?.Tick();
    }

    public void UpdateRootMotionDelta(Vector3 delta)
    {
        // Skip applying near-zero root motion to avoid blocking manual movement (e.g., aiming state).
        if (delta.sqrMagnitude > 0.000001f)
        {
            Movement.Move(delta);
        }
    }

    void OnEnable()
    {
        Input.ReloadTriggered += HandleReload;
        Input.NextWeaponTriggered += HandleNextWeapon;
        Input.PreviousWeaponTriggered += HandlePreviousWeapon;
        Input.InteractTriggered += HandleInteract;
        if (WeaponLoadoutController != null)
        {
            WeaponLoadoutController.WeaponActivated += HandleWeaponActivated;
        }
    }

    void OnDisable()
    {
        Input.ReloadTriggered -= HandleReload;
        Input.NextWeaponTriggered -= HandleNextWeapon;
        Input.PreviousWeaponTriggered -= HandlePreviousWeapon;
        Input.InteractTriggered -= HandleInteract;
        if (WeaponLoadoutController != null)
        {
            WeaponLoadoutController.WeaponActivated -= HandleWeaponActivated;
        }
        Input.Dispose();
    }

    void HandleReload() => WeaponLoadoutController.CurrentWeapon.Reload();

    void HandleNextWeapon() => WeaponLoadoutController.EquipNext(1);
    void HandlePreviousWeapon() => WeaponLoadoutController.EquipNext(-1);

    void HandleWeaponActivated(WeaponLoadout.WeaponSlot slot)
    {
        SetAnimatorWeaponType(slot.Instance);
    }

    void HandleInteract()
    {
        Debug.Log("Interact!");
        interactionSensor?.Interact();
    }

    void SetAnimatorWeaponType(Weapon weapon)
    {
        float weaponType = 0f;
        if (weapon != null && weapon.Model != null && weapon.Model.Data != null)
        {
            weaponType = weapon.Model.Data.type;
        }

        AnimatorController.SetFloat("WeaponType", weaponType);
    }
}
