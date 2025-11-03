using UnityEngine;

public enum ItemType
{
    Consumable,
    Ammo,
    Weapon,
    Misc
}

/// <summary>
/// Base definition for all items. Specific categories derive from this class.
/// </summary>
public abstract class ItemData : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField, Min(1)] private int maxStack = 1;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public virtual int MaxStack => Mathf.Max(1, maxStack);
    public abstract ItemType Type { get; }
}

[CreateAssetMenu(fileName = "ConsumableItem", menuName = "GameData/Items/Consumable")]
public class ConsumableItemData : ItemData
{
    [SerializeField] private float healAmount;
    [SerializeField, Tooltip("Seconds before this consumable can be used again.")] private float cooldownSeconds;

    public float HealAmount => healAmount;
    public float CooldownSeconds => Mathf.Max(0f, cooldownSeconds);
    public override ItemType Type => ItemType.Consumable;
}

[CreateAssetMenu(fileName = "AmmoItem", menuName = "GameData/Items/Ammo")]
public class AmmoItemData : ItemData
{
    [SerializeField, Tooltip("Identifier used by weapons to request compatible ammo.")]
    private string ammoId;
    [SerializeField, Tooltip("Amount of ammo contained in one item stack.")] private int ammoAmount = 1;

    public string AmmoId => ammoId;
    public int AmmoAmount => Mathf.Max(1, ammoAmount);
    public override ItemType Type => ItemType.Ammo;
}

[CreateAssetMenu(fileName = "WeaponItem", menuName = "GameData/Items/Weapon")]
public class WeaponItemData : ItemData
{
    [SerializeField] private Weapon weaponPrefab;

    public Weapon WeaponPrefab => weaponPrefab;
    public override ItemType Type => ItemType.Weapon;
    public override int MaxStack => 1;
}
