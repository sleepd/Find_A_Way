using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Binds an InventoryModel to a list of slot views. Attach to the slots container and assign a slot prefab.
/// </summary>
public class UIInventoryView : MonoBehaviour
{
    [SerializeField] private Transform slotsRoot;
    [SerializeField] private GameObject slotPrefab;

    private InventoryModel _model;
    private readonly List<UIInventorySlotView> _slots = new();
    private Action<int> _slotClicked;

    void Awake()
    {
        if (slotsRoot == null)
        {
            slotsRoot = transform;
        }
    }

    void OnEnable()
    {
        Subscribe();
        RefreshAllSlots();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void OnDestroy()
    {
        Unsubscribe();
    }

    /// <summary>
    /// Bind to an inventory model and rebuild slots to match its size.
    /// </summary>
    public void Bind(InventoryModel model)
    {
        if (_model == model)
        {
            RefreshAllSlots();
            return;
        }

        Unsubscribe();
        _model = model;

        if (_model == null)
        {
            ClearSlots();
            return;
        }

        RebuildSlots();
        Subscribe();
        RefreshAllSlots();
    }

    public void SetOnSlotClicked(Action<int> onClicked)
    {
        _slotClicked = onClicked;
        // Update existing slots with new callback.
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i]?.Initialize(i, _slotClicked);
        }
    }

    private void Subscribe()
    {
        if (_model == null)
        {
            return;
        }

        _model.SlotChanged += HandleSlotChanged;
    }

    private void Unsubscribe()
    {
        if (_model == null)
        {
            return;
        }

        _model.SlotChanged -= HandleSlotChanged;
    }

    private void RebuildSlots()
    {
        ClearSlots();

        if (slotPrefab == null || slotsRoot == null || _model == null)
        {
            return;
        }

        for (int i = 0; i < _model.Slots.Count; i++)
        {
            var slotGO = Instantiate(slotPrefab, slotsRoot);
            var slotView = slotGO.GetComponent<UIInventorySlotView>();
            if (slotView == null)
            {
                slotView = slotGO.AddComponent<UIInventorySlotView>();
            }
            slotView.Initialize(i, _slotClicked);
            _slots.Add(slotView);
        }
    }

    private void ClearSlots()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] != null)
            {
                Destroy(_slots[i].gameObject);
            }
        }
        _slots.Clear();
    }

    private void RefreshAllSlots()
    {
        if (_model == null)
        {
            return;
        }

        var slots = _model.Slots;
        for (int i = 0; i < _slots.Count && i < slots.Count; i++)
        {
            _slots[i].SetSlot(slots[i]);
        }
    }

    private void HandleSlotChanged(int index, InventoryModel.InventorySlot slot)
    {
        if (index < 0 || index >= _slots.Count)
        {
            return;
        }

        _slots[index].SetSlot(slot);
    }
}
