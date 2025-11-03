using System;

/// <summary>
/// Pure logic controller that works hand-in-hand with <see cref="WeaponLoadout"/>.
/// Responsible for translating gameplay input (fire, reload, equip) into model state
/// while exposing events for driving actual Weapon MonoBehaviours or UI.
/// </summary>
public sealed class WeaponController
{
    private readonly WeaponLoadout _loadout;
    private bool _isFiring;

    public WeaponController(WeaponLoadout loadout)
    {
        _loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
        _loadout.CurrentIndexChanged += HandleCurrentIndexChanged;
    }

    public WeaponLoadout Loadout => _loadout;
    public WeaponLoadout.WeaponSlot CurrentSlot => _loadout.CurrentSlot;
    public WeaponModel CurrentModel => CurrentSlot.Model;
    public WeaponData CurrentData => CurrentSlot.Data;
    public bool IsFiring => _isFiring;

    public event Action<WeaponLoadout.WeaponSlot> WeaponActivated;
    public event Action<WeaponLoadout.WeaponSlot> WeaponDeactivated;
    public event Action Fired;
    public event Action ReloadStarted;
    public event Action ReloadCanceled;
    public event Action ReloadCompleted;

    public bool BeginFire()
    {
        var model = CurrentModel;
        if (model == null)
        {
            return false;
        }

        if (_isFiring)
        {
            return false;
        }

        if (!model.TryFire())
        {
            return false;
        }

        _isFiring = true;
        Fired?.Invoke();
        return true;
    }

    public void EndFire()
    {
        _isFiring = false;
        var model = CurrentModel;
        model?.CancelReload();
    }

    public bool TryTick(float deltaTime)
    {
        var model = CurrentModel;
        if (model == null)
        {
            return false;
        }

        model.Tick(deltaTime);
        if (_isFiring && !model.TryFire())
        {
            _isFiring = false;
        }

        return true;
    }

    public bool StartReload()
    {
        var model = CurrentModel;
        if (model == null || model.IsReloading)
        {
            return false;
        }

        model.StartReload();
        ReloadStarted?.Invoke();
        return true;
    }

    public void CancelReload()
    {
        var model = CurrentModel;
        if (model == null || !model.IsReloading)
        {
            return;
        }

        model.CancelReload();
        ReloadCanceled?.Invoke();
    }

    public bool EquipSlot(int index)
    {
        var previous = _loadout.CurrentSlot;
        if (!_loadout.SelectIndex(index))
        {
            return false;
        }

        WeaponDeactivated?.Invoke(previous);
        WeaponActivated?.Invoke(_loadout.CurrentSlot);
        _isFiring = false;
        return true;
    }

    public bool EquipNext(int direction = 1) => EquipWithDelegate(() => _loadout.SelectNext(direction));

    private bool EquipWithDelegate(Func<bool> changeFunc)
    {
        var previous = _loadout.CurrentSlot;
        if (!changeFunc())
        {
            return false;
        }

        WeaponDeactivated?.Invoke(previous);
        WeaponActivated?.Invoke(_loadout.CurrentSlot);
        _isFiring = false;
        return true;
    }

    private void HandleCurrentIndexChanged(int newIndex)
    {
        // In case external code set the index directly on the loadout
        WeaponActivated?.Invoke(_loadout.CurrentSlot);
        _isFiring = false;
    }
}
