
using UnityEngine;

/// <summary>
/// Runtime representation of a weapon. Wraps static configuration from WeaponData
/// and keeps track of mutable state such as ammo, cooldown, and reload progress.
/// </summary>
public class WeaponModel
{
    private float reloadTimer;

    public WeaponModel(WeaponData data)
    {
        Data = data != null ? data : throw new System.ArgumentNullException(nameof(data));
        CurrentAmmo = Mathf.Max(0, Data.magazineSize);
        FireCooldown = 0f;
        reloadTimer = 0f;
        AmmoChanged?.Invoke(CurrentAmmo, Data.magazineSize);
    }

    public WeaponData Data { get; }
    public int CurrentAmmo { get; private set; }
    public float FireCooldown { get; private set; }
    public bool IsReloading { get; private set; }
    public bool HasAmmoInMagazine => CurrentAmmo > 0;
    public bool IsOnCooldown => FireCooldown > 0f;
    public float FireInterval => Data.fireRate > 0f ? 1f / Data.fireRate : 0f;
    public float ReloadProgress => IsReloading && Data.reloadTime > 0f
        ? Mathf.Clamp01(reloadTimer / Data.reloadTime)
        : 0f;
    public float MoveSpeedReduction => Mathf.Clamp01(Data.moveSpeedReduction);
    public float MoveSpeedMultiplier => Mathf.Clamp01(1f - MoveSpeedReduction);
    public float BulletSpeed => Mathf.Max(0f, Data.bulletSpeed);
    public float MaxRange => Mathf.Max(0f, Data.maxRange);
    public float ProjectileLifetime => BulletSpeed > 0f ? MaxRange / BulletSpeed : 0f;
    public event System.Action<int, int> AmmoChanged;
    public event System.Action ReloadStarted;
    public event System.Action<float> ReloadProgressChanged;
    public event System.Action ReloadCompleted;
    public event System.Action ReloadCanceled;
    public event System.Action<int, int> ReloadStepFilled;

    public bool CanFire =>
        !IsReloading &&
        !IsOnCooldown &&
        HasAmmoInMagazine;

    public void ResetState()
    {
        CurrentAmmo = Mathf.Max(0, Data.magazineSize);
        FireCooldown = 0f;
        reloadTimer = 0f;
        IsReloading = false;
        AmmoChanged?.Invoke(CurrentAmmo, Data.magazineSize);
        ReloadProgressChanged?.Invoke(0f);
    }

    /// <summary>
    /// Call every frame/tick to update timers.
    /// </summary>
    public void Tick(float deltaTime)
    {
        if (FireCooldown > 0f)
        {
            FireCooldown = Mathf.Max(0f, FireCooldown - deltaTime);
        }

        if (IsReloading)
        {
            reloadTimer += deltaTime;

            if (Data.reloadTime > 0f)
            {
                var progress = Mathf.Clamp01(reloadTimer / Mathf.Max(float.Epsilon, Data.reloadTime));
                ReloadProgressChanged?.Invoke(progress);
            }

            if (reloadTimer >= Mathf.Max(0f, Data.reloadTime))
            {
                CompleteReload();
            }
        }
    }

    public bool TryFire()
    {
        if (!CanFire)
        {
            return false;
        }

        CurrentAmmo = Mathf.Max(0, CurrentAmmo - 1);
        FireCooldown = FireInterval;
        AmmoChanged?.Invoke(CurrentAmmo, Data.magazineSize);

        return true;
    }

    public void StartReload()
    {
        if (IsReloading || CurrentAmmo >= Data.magazineSize)
        {
            return;
        }

        if (Data.reloadTime <= 0f)
        {
            IsReloading = true;
            reloadTimer = 0f;
            ReloadStarted?.Invoke();
            ReloadProgressChanged?.Invoke(0f);
            CompleteReload();
            return;
        }

        IsReloading = true;
        reloadTimer = 0f;
        ReloadStarted?.Invoke();
        ReloadProgressChanged?.Invoke(0f);
    }

    public void CancelReload()
    {
        if (!IsReloading)
        {
            return;
        }

        IsReloading = false;
        reloadTimer = 0f;
        ReloadCanceled?.Invoke();
        ReloadProgressChanged?.Invoke(0f);
    }

    private void CompleteReload()
    {
        IsReloading = false;
        reloadTimer = 0f;

        switch (Data.reloadMode)
        {
            case ReloadMode.Magazine:
                CurrentAmmo = Mathf.Max(0, Data.magazineSize);
                AmmoChanged?.Invoke(CurrentAmmo, Data.magazineSize);
                ReloadProgressChanged?.Invoke(1f);
                ReloadCompleted?.Invoke();
                break;
            case ReloadMode.PerBullet:
                CurrentAmmo = Mathf.Min(Data.magazineSize, CurrentAmmo + 1);
                AmmoChanged?.Invoke(CurrentAmmo, Data.magazineSize);
                ReloadStepFilled?.Invoke(CurrentAmmo, Data.magazineSize);
                ReloadProgressChanged?.Invoke(1f);
                if (CurrentAmmo < Data.magazineSize)
                {
                    // Continue the per-bullet reload loop automatically until full.
                    StartReload();
                }
                else
                {
                    ReloadCompleted?.Invoke();
                }
                break;
        }
    }

    public void AmmoState()
    {
        AmmoChanged?.Invoke(CurrentAmmo, Data.magazineSize);
    }
}
