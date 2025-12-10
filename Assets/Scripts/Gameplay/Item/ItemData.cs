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
