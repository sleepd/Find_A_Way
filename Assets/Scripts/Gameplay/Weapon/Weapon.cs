using UnityEngine;

public class Weapon : MonoBehaviour, IWeapon
{
    [SerializeField] WeaponData _weaponData;
    [SerializeField] Transform _firePoint;
    [SerializeField] BulletPool _bulletPool;
    [Header("Audio")]
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _fireClip;
    [SerializeField] AudioClip _reloadClip;
    [SerializeField, Range(0f, 1f)] float _fireVolume = 1f;
    [SerializeField, Range(0f, 1f)] float _reloadVolume = 1f;
    public WeaponModel Model { get; private set; }
    public bool IsFiring { get; private set; }
    private PlayerController _playerController;

    void Awake()
    {
        Model = new(_weaponData);
    }

    public void SetPlayer(PlayerController playerController)
    {
        _playerController = playerController;
    }

    // Update is called once per frame
    void Update()
    {
        Model.Tick(Time.deltaTime);

        if(IsFiring)
        {
            if(Model.TryFire())
            {
                Use();
            }
        }
    }

    public void BeginFire()
    {
        IsFiring = true;
    }

    public void EndFire()
    {
        IsFiring = false;
    }

    public void Reload()
    {
        if (Model == null)
        {
            return;
        }

        var ammoBefore = Model.CurrentAmmo;
        var wasReloading = Model.IsReloading;

        Model.StartReload();

        var startedReload = Model.IsReloading && !wasReloading;
        var ammoIncreased = Model.CurrentAmmo > ammoBefore;

        if (startedReload || ammoIncreased)
        {
            PlayReloadSound();
        }
    }
    
    public void Use()
    {
        var pool = _bulletPool != null ? _bulletPool : BulletPool.Instance;
        if (pool == null)
        {
            Debug.LogWarning("Attempted to fire weapon but no BulletPool is available.", this);
            return;
        }

        if (_firePoint == null)
        {
            Debug.LogWarning("Weapon has no fire point assigned.", this);
            return;
        }

        PlayFireSound();

        var spawnPosition = _firePoint.position;
        var direction = _playerController.transform.forward;
        var playerPosition = _playerController.transform.position;
        var rayOrigin = new Vector3(playerPosition.x, spawnPosition.y, playerPosition.z);

        var bulletsPerShot = Mathf.Max(1, Model.Data.bulletsPerShot);
        var bulletSpeed = Model.BulletSpeed;
        var spreadAngle = Mathf.Max(0f, Model.Data.spreadAngle);
        var maxRange = Model.MaxRange;

        for (var i = 0; i < bulletsPerShot; i++)
        {
            var spreadDirection = ApplySpread(direction, spreadAngle);
            var maxDistance = maxRange > 0f ? maxRange : Mathf.Infinity;
            var ray = new Ray(rayOrigin, spreadDirection);
            Vector3 targetPosition;

            if (Physics.Raycast(ray, out var hitInfo, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                targetPosition = hitInfo.point;
            }
            else
            {
                var fallbackDistance = maxRange > 0f ? maxRange : 1000f;
                targetPosition = spawnPosition + spreadDirection * fallbackDistance;
            }

            var bullet = pool.Get();
            bullet.Initialize(spawnPosition, targetPosition, bulletSpeed);
        }
    }

    private void PlayFireSound()
    {
        if (_fireClip == null)
        {
            return;
        }

        if (_audioSource != null)
        {
            _audioSource.PlayOneShot(_fireClip, _fireVolume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(_fireClip, _firePoint != null ? _firePoint.position : transform.position, _fireVolume);
        }
    }

    private static Vector3 ApplySpread(Vector3 direction, float spreadAngle)
    {
        if (spreadAngle <= 0.001f)
        {
            return direction.normalized;
        }

        var randomYaw = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
        var rotation = Quaternion.Euler(0f, randomYaw, 0f);
        return (rotation * direction).normalized;
    }

    private void PlayReloadSound()
    {
        if (_reloadClip == null)
        {
            return;
        }

        if (_audioSource != null)
        {
            _audioSource.PlayOneShot(_reloadClip, _reloadVolume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(_reloadClip, _firePoint != null ? _firePoint.position : transform.position, _reloadVolume);
        }
    }
}
