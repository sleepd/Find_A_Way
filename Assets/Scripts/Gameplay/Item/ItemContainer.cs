using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// World container that holds items players can withdraw. Emits events for UI synchronization.
/// </summary>
public class ItemContainer : MonoBehaviour, IInteractable
{
    [Serializable]
    private struct InitialEntry
    {
        public ItemData item;
        [Min(1)] public int amount;
    }

    [SerializeField, Min(1)]
    private int slotCount = 4;
    [SerializeField] private InitialEntry[] initialItems;
    [Header("Interaction")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float focusRadius = 4f;
    [SerializeField] private float interactRadius = 2f;
    [SerializeField] private string displayName = "Container";

    private InventoryModel _model;

    public InventoryModel Model => _model;
    public IReadOnlyList<InventoryModel.InventorySlot> Slots => _model.Slots;
    public Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;
    public float FocusRadius => Mathf.Max(0.1f, focusRadius);
    public float InteractRadius => Mathf.Max(0.1f, interactRadius);
    public string DisplayName => displayName;

    public event Action<ItemData, int> ItemAdded;
    public event Action<ItemData, int> ItemRemoved;
    public event Action<int, InventoryModel.InventorySlot> SlotChanged;

    void Awake()
    {
        _model = new InventoryModel(Mathf.Max(1, slotCount));
        _model.ItemAdded += OnItemAddedToModel;
        _model.ItemRemoved += OnItemRemovedFromModel;
        _model.SlotChanged += OnSlotChanged;
        ApplyInitialItems();
    }

    private void ApplyInitialItems()
    {
        if (initialItems == null)
        {
            return;
        }

        foreach (var entry in initialItems)
        {
            if (entry.item == null || entry.amount <= 0)
            {
                continue;
            }

            _model.AddItem(entry.item, entry.amount);
        }
    }

    public int AddItem(ItemData item, int amount)
    {
        return _model.AddItem(item, amount);
    }

    public int TakeItem(ItemData item, int amount)
    {
        return _model.RemoveItem(item, amount);
    }

    public bool TryTakeFromSlot(int index, int amount, out ItemData item, out int removed)
    {
        removed = 0;
        item = null;

        if (index < 0 || index >= _model.Slots.Count)
        {
            return false;
        }

        var slot = _model.Slots[index];
        if (slot.IsEmpty)
        {
            return false;
        }

        item = slot.Item;
        removed = _model.RemoveItem(item, amount);
        return removed > 0;
    }

    public void Clear()
    {
        _model.Clear();
    }

    public void BeginFocus(PlayerController player)
    {
        // Placeholder; add highlight or indicator here if desired.
    }

    public void EndFocus(PlayerController player)
    {
        var dialog = UnityEngine.Object.FindFirstObjectByType<UILootDialog>(FindObjectsInactive.Include);
        if (dialog != null && dialog.CurrentContainer == this)
        {
            dialog.Hide();
            var menu = UnityEngine.Object.FindFirstObjectByType<UIInGameMenu>(FindObjectsInactive.Include);
            menu?.HideInventoryPanel();
        }
    }

    public void Interact(PlayerController player)
    {
        var dialog = UnityEngine.Object.FindFirstObjectByType<UILootDialog>(FindObjectsInactive.Include);
        if (dialog != null)
        {
            dialog.Show(this);
            var menu = UnityEngine.Object.FindFirstObjectByType<UIInGameMenu>(FindObjectsInactive.Include);
            menu?.ShowInventoryPanel();
        }
        else
        {
            Debug.LogWarning("UILootDialog not found in scene; cannot display loot UI.");
        }
    }

    void OnDestroy()
    {
        if (_model != null)
        {
            _model.ItemAdded -= OnItemAddedToModel;
            _model.ItemRemoved -= OnItemRemovedFromModel;
            _model.SlotChanged -= OnSlotChanged;
        }
    }

    private void OnItemAddedToModel(ItemData item, int amount)
    {
        ItemAdded?.Invoke(item, amount);
    }

    private void OnItemRemovedFromModel(ItemData item, int amount)
    {
        ItemRemoved?.Invoke(item, amount);
    }

    private void OnSlotChanged(int index, InventoryModel.InventorySlot slot)
    {
        SlotChanged?.Invoke(index, slot);
    }
}
