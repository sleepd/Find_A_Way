using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pure runtime model that tracks the player's equipped weapon slots, current selection,
/// and surviving runtime state (WeaponModel). UI or controllers can subscribe to its events.
/// </summary>
public sealed class WeaponLoadout
{
    private readonly WeaponSlot[] _slots;
    private int _currentIndex;

    public WeaponLoadout(int slotCount)
    {
        if (slotCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slotCount), "Loadout must contain at least one slot.");
        }

        _slots = new WeaponSlot[slotCount];
        for (var i = 0; i < slotCount; i++)
        {
            _slots[i] = WeaponSlot.Empty;
        }

        _currentIndex = 0;
    }

    public IReadOnlyList<WeaponSlot> Slots => _slots;
    public int SlotCount => _slots.Length;
    public int CurrentIndex => _currentIndex;
    public WeaponSlot CurrentSlot => IsValidIndex(_currentIndex) ? _slots[_currentIndex] : WeaponSlot.Empty;

    public event Action<int, WeaponSlot> SlotChanged;
    public event Action<int> CurrentIndexChanged;
    public event Action<int, WeaponSlot> WeaponEquipped;
    public event Action<int, WeaponSlot> WeaponUnequipped;

    public bool SetSlot(int index, WeaponData weaponData, WeaponModel weaponModel = null)
    {
        if (!IsValidIndex(index))
        {
            return false;
        }

        var previous = _slots[index];
        var nextSlot = weaponData == null
            ? WeaponSlot.Empty
            : WeaponSlot.FromData(weaponData, weaponModel ?? new WeaponModel(weaponData));

        if (previous.Equals(nextSlot))
        {
            return true;
        }

        _slots[index] = nextSlot;
        SlotChanged?.Invoke(index, nextSlot);

        if (!previous.IsEmpty)
        {
            WeaponUnequipped?.Invoke(index, previous);
        }

        if (!nextSlot.IsEmpty)
        {
            WeaponEquipped?.Invoke(index, nextSlot);
        }

        if (_currentIndex == index && nextSlot.IsEmpty)
        {
            CycleToNextAvailable();
        }

        return true;
    }

    public bool ClearSlot(int index) => SetSlot(index, null);

    public bool SelectIndex(int index)
    {
        if (!IsValidIndex(index) || index == _currentIndex || _slots[index].IsEmpty)
        {
            return false;
        }

        _currentIndex = index;
        CurrentIndexChanged?.Invoke(_currentIndex);
        return true;
    }

    public bool SelectNext(int direction = 1)
    {
        if (SlotCount <= 1)
        {
            return false;
        }

        direction = direction >= 0 ? 1 : -1;
        var startIndex = _currentIndex;
        var current = _currentIndex;

        do
        {
            current = (current + direction + SlotCount) % SlotCount;
            if (!_slots[current].IsEmpty)
            {
                _currentIndex = current;
                CurrentIndexChanged?.Invoke(_currentIndex);
                return true;
            }
        }
        while (current != startIndex);

        return false;
    }

    private void CycleToNextAvailable()
    {
        if (!SelectNext(1))
        {
            _currentIndex = 0;
            CurrentIndexChanged?.Invoke(_currentIndex);
        }
    }

    private bool IsValidIndex(int index) => index >= 0 && index < _slots.Length;

    public readonly struct WeaponSlot : IEquatable<WeaponSlot>
    {
        public static readonly WeaponSlot Empty = new WeaponSlot(null, null);

        public WeaponSlot(WeaponData data, WeaponModel model)
        {
            Data = data;
            Model = model;
        }

        public WeaponData Data { get; }
        public WeaponModel Model { get; }
        public bool IsEmpty => Data == null || Model == null;

        public static WeaponSlot FromData(WeaponData data, WeaponModel model)
        {
            if (data == null)
            {
                return Empty;
            }

            return new WeaponSlot(data, model ?? new WeaponModel(data));
        }

        public bool Equals(WeaponSlot other) => Data == other.Data && Model == other.Model;

        public override bool Equals(object obj) => obj is WeaponSlot other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Data != null ? Data.GetHashCode() : 0) * 397) ^ (Model != null ? Model.GetHashCode() : 0);
            }
        }
    }
}
