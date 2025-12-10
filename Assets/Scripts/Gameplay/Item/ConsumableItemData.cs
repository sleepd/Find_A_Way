using UnityEngine;

[CreateAssetMenu(fileName = "ConsumableItem", menuName = "GameData/Items/Consumable")]
public class ConsumableItemData : ItemData
{
    [SerializeField] private float healAmount;
    [SerializeField, Tooltip("Seconds before this consumable can be used again.")] private float cooldownSeconds;

    public float HealAmount => healAmount;
    public float CooldownSeconds => Mathf.Max(0f, cooldownSeconds);
    public override ItemType Type => ItemType.Consumable;
}
