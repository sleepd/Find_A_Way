using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime representation of the player's inventory. Supports stackable items
/// with per-item stack limits and basic add/remove/query operations.
/// </summary>
public class InventoryModel
{
    private readonly List<InventorySlot> _slots;

    public InventoryModel(int slotCount)
    {
        if (slotCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slotCount), "Inventory must have at least one slot.");
        }

        _slots = new List<InventorySlot>(slotCount);

        for (int i = 0; i < slotCount; i++)
        {
            _slots.Add(InventorySlot.Empty);
        }
    }

    public IReadOnlyList<InventorySlot> Slots => _slots;

    public event Action<int, InventorySlot> SlotChanged;
    public event Action<ItemData, int> ItemAdded;
    public event Action<ItemData, int> ItemRemoved;

    /// <summary>
    /// Attempts to add items into the inventory, respecting stack limits.
    /// Returns how many items were successfully added.
    /// </summary>
    public int AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return 0;
        }

        int remaining = amount;
        int addedTotal = 0;

        // Step 1: Try to fill existing stacks.
        for (int i = 0; i < _slots.Count && remaining > 0; i++)
        {
            var slot = _slots[i];
            if (!slot.IsEmpty && slot.Item == item)
            {
                int added = slot.Add(item, remaining);
                if (added > 0)
                {
                    SetSlot(i, slot);
                    remaining -= added;
                    addedTotal += added;
                }
            }
        }

        // Step 2: Fill empty slots.
        for (int i = 0; i < _slots.Count && remaining > 0; i++)
        {
            var slot = _slots[i];
            if (slot.IsEmpty)
            {
                var newSlot = new InventorySlot(item, 0);
                int added = newSlot.Add(item, remaining);
                if (added > 0)
                {
                    SetSlot(i, newSlot);
                    remaining -= added;
                    addedTotal += added;
                }
            }
        }

        if (addedTotal > 0)
        {
            ItemAdded?.Invoke(item, addedTotal);
        }

        return amount - remaining;
    }

    /// <summary>
    /// Removes up to <paramref name="amount"/> items matching <paramref name="item"/>.
    /// Returns the quantity actually removed.
    /// </summary>
    public int RemoveItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return 0;
        }

        int remaining = amount;
        int removedTotal = 0;

        for (int i = 0; i < _slots.Count && remaining > 0; i++)
        {
            var slot = _slots[i];
            if (!slot.IsEmpty && slot.Item == item)
            {
                int removed = slot.Remove(remaining);
                if (removed > 0)
                {
                    remaining -= removed;
                    removedTotal += removed;
                    if (slot.IsEmpty)
                    {
                        SetSlot(i, InventorySlot.Empty);
                    }
                    else
                    {
                        SetSlot(i, slot);
                    }
                }
            }
        }

        if (removedTotal > 0)
        {
            ItemRemoved?.Invoke(item, removedTotal);
        }

        return amount - remaining;
    }

    /// <summary>
    /// Checks total quantity of the given item across the inventory.
    /// </summary>
    public int CountItem(ItemData item)
    {
        if (item == null)
        {
            return 0;
        }

        int total = 0;

        foreach (var slot in _slots)
        {
            if (!slot.IsEmpty && slot.Item == item)
            {
                total += slot.Count;
            }
        }

        return total;
    }

    /// <summary>
    /// Tries to find the first slot containing the given item.
    /// </summary>
    public bool TryGetSlot(ItemData item, out int slotIndex, out InventorySlot slot)
    {
        if (item == null)
        {
            slotIndex = -1;
            slot = InventorySlot.Empty;
            return false;
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            var current = _slots[i];
            if (!current.IsEmpty && current.Item == item)
            {
                slotIndex = i;
                slot = current;
                return true;
            }
        }

        slotIndex = -1;
        slot = InventorySlot.Empty;
        return false;
    }

    /// <summary>
    /// Clears all slots.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (!_slots[i].IsEmpty)
            {
                SetSlot(i, InventorySlot.Empty);
            }
        }
    }

    private void SetSlot(int index, InventorySlot slot)
    {
        _slots[index] = slot;
        SlotChanged?.Invoke(index, slot);
    }

    public struct InventorySlot
    {
        public static readonly InventorySlot Empty = new InventorySlot(null, 0);

        public InventorySlot(ItemData item, int count)
        {
            Item = item;
            Count = count;
        }

        public ItemData Item { get; }
        public int Count { get; private set; }
        public bool IsEmpty => Item == null || Count <= 0;

        public bool IsFull
        {
            get
            {
                if (IsEmpty || Item == null)
                {
                    return false;
                }
                var maxStack = Mathf.Max(1, Item.MaxStack);
                return Count >= maxStack;
            }
        }

        public int Add(ItemData item, int amount)
        {
            if (item == null || amount <= 0)
            {
                return 0;
            }

            if (IsEmpty)
            {
                var maxStack = Mathf.Max(1, item.MaxStack);
                Count = Mathf.Clamp(amount, 0, maxStack);
                return Count;
            }

            if (Item != item || IsFull)
            {
                return 0;
            }

            var capacity = Mathf.Max(1, Item.MaxStack);
            int space = capacity - Count;
            int added = Mathf.Min(space, amount);
            Count += added;
            return added;
        }

        public int Remove(int amount)
        {
            if (IsEmpty || amount <= 0)
            {
                return 0;
            }

            int removed = Mathf.Min(amount, Count);
            Count -= removed;
            if (Count <= 0)
            {
                Count = 0;
            }

            return removed;
        }
    }
}
