using UnityEngine;

[CreateAssetMenu(fileName = "WeaponItem", menuName = "GameData/Items/Weapon")]
public class WeaponItemData : ItemData
{
    [SerializeField] private Weapon weaponPrefab;

    public Weapon WeaponPrefab => weaponPrefab;
    public override ItemType Type => ItemType.Weapon;
    public override int MaxStack => 1;
}
