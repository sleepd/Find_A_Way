using System;
using UnityEngine;

/// <summary>
/// Coordinates loadout selection with the weapon instances tracked by <see cref="WeaponLoadout"/>,
/// toggling the relevant GameObjects and exposing activation events for other systems.
/// </summary>
public sealed class WeaponLoadoutController
{
    private readonly WeaponLoadout _loadout;
    private WeaponLoadout.WeaponSlot _activeSlot;

    public WeaponLoadoutController(WeaponLoadout loadout)
    {
        _loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
        _loadout.CurrentIndexChanged += HandleCurrentIndexChanged;
        _loadout.WeaponEquipped += HandleWeaponEquipped;
        _loadout.WeaponUnequipped += HandleWeaponUnequipped;
        _activeSlot = _loadout.CurrentSlot;
        if (!_activeSlot.IsEmpty)
        {
            SetSlotActive(_activeSlot, true);
        }
    }

    public WeaponLoadout Loadout => _loadout;
    public WeaponLoadout.WeaponSlot CurrentSlot => _loadout.CurrentSlot;
    public Weapon CurrentWeapon => CurrentSlot.Instance;

    public event Action<WeaponLoadout.WeaponSlot> WeaponActivated;
    public event Action<WeaponLoadout.WeaponSlot> WeaponDeactivated;

    public bool AssignWeapon(int index, Weapon weapon)
    {
        return _loadout.SetSlot(index, weapon);
    }

    public bool ClearSlot(int index) => _loadout.ClearSlot(index);

    public bool EquipSlot(int index)
    {
        return _loadout.SelectIndex(index);
    }

    public bool EquipNext(int direction = 1) => _loadout.SelectNext(direction);

    private void HandleCurrentIndexChanged(int newIndex)
    {
        var previous = _activeSlot;
        var current = _loadout.CurrentSlot;

        if (!previous.Equals(current))
        {
            SetSlotActive(previous, false);
            _activeSlot = current;
            SetSlotActive(_activeSlot, true);
        }
        else if (!current.IsEmpty)
        {
            // Ensure current weapon stays active even if loadout triggered event redundantly.
            current.Instance.gameObject.SetActive(true);
        }
    }

    private void SetSlotActive(WeaponLoadout.WeaponSlot slot, bool active)
    {
        if (slot.IsEmpty)
        {
            return;
        }

        var weapon = slot.Instance;
        if (weapon == null)
        {
            return;
        }

        if (!active)
        {
            weapon.EndFire();
            var model = weapon.Model;
            if (model != null && model.IsReloading)
            {
                model.CancelReload();
            }
            weapon.gameObject.SetActive(false);
            WeaponDeactivated?.Invoke(slot);
            return;
        }

        weapon.gameObject.SetActive(true);
        WeaponActivated?.Invoke(slot);
    }

    private void HandleWeaponEquipped(int index, WeaponLoadout.WeaponSlot slot)
    {
        if (index != _loadout.CurrentIndex)
        {
            return;
        }

        _activeSlot = slot;
        SetSlotActive(slot, true);
    }

    private void HandleWeaponUnequipped(int index, WeaponLoadout.WeaponSlot slot)
    {
        if (slot.IsEmpty || !_activeSlot.Equals(slot))
        {
            return;
        }

        SetSlotActive(slot, false);

        _activeSlot = WeaponLoadout.WeaponSlot.Empty;
    }
}
