using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "GameData/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string WeaponName;
    public Sprite sprite;
    [Tooltip("Number of times the weapon can fire per second.")]
    public float fireRate;
    public int bulletsPerShot;
    public int magazineSize;
    public float damage;
    public float criticalChance;
    public float criticalMultiplier;
    public float spreadAngle;
    public float recoil;
    public float reloadTime;
    public ReloadMode reloadMode;
    [Tooltip("Slowdown percentage applied while this weapon is equipped.")]
    public float moveSpeedReduction;
    public float bulletSpeed;
    [Tooltip("Maximum range in meters before projectiles despawn.")]
    public float maxRange;
}

public enum ReloadMode
{
    PerBullet,
    Magazine
}
