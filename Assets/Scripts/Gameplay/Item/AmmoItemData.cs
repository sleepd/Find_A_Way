using UnityEngine;

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
