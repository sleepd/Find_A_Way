using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight helper around <see cref="InventoryModel"/> that exposes a simpler API.
/// </summary>
public sealed class InventoryManager
{
    private readonly InventoryModel _model;

    public InventoryManager(int slotCount)
    {
        _model = new InventoryModel(Mathf.Max(1, slotCount));
    }

    public InventoryModel Model => _model;
    public IReadOnlyList<InventoryModel.InventorySlot> Slots => _model.Slots;
    public int SlotCount => _model.Slots.Count;

    public int AddItem(ItemData item, int amount) => _model.AddItem(item, amount);
    public int RemoveItem(ItemData item, int amount) => _model.RemoveItem(item, amount);
    public int CountItem(ItemData item) => _model.CountItem(item);
    public bool TryGetSlot(ItemData item, out int slotIndex, out InventoryModel.InventorySlot slot) =>
        _model.TryGetSlot(item, out slotIndex, out slot);

    public bool TryGetSlot(int index, out InventoryModel.InventorySlot slot)
    {
        if (index < 0 || index >= _model.Slots.Count)
        {
            slot = InventoryModel.InventorySlot.Empty;
            return false;
        }

        slot = _model.Slots[index];
        return true;
    }

    public void Clear() => _model.Clear();
}
