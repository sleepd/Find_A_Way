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
    [SerializeField, Tooltip("Degrees per second the weapon rotates toward the aim point.")]
    private float _aimRotationSpeed = 720f;
    [SerializeField, Tooltip("Minimum horizontal distance between muzzle and aim point before rotating.")]
    private float _minAimDistance = 0.3f;
    public WeaponModel Model { get; private set; }
    public bool IsFiring { get; private set; }
    private PlayerController _playerController;
    private Camera _camera;

    void OnEnable()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        if (Model != null)
        {
            SubscribeModelEvents(Model);
        }
    }

    void Awake()
    {
        Model = new(_weaponData);
        _camera = Camera.main;
        SubscribeModelEvents(Model);
    }

#region Model Event Subscription
    void OnDisable()
    {
        if (Model != null)
        {
            UnsubscribeModelEvents(Model);
        }
    }

    void OnDestroy()
    {
        if (Model != null)
        {
            UnsubscribeModelEvents(Model);
        }
    }

    void SubscribeModelEvents(WeaponModel model)
    {
        if (model == null)
        {
            return;
        }

        model.ReloadStarted += HandleReloadStarted;
        model.ReloadStepFilled += HandleReloadStepFilled;
        model.ReloadCompleted += HandleReloadCompleted;
        model.ReloadCanceled += HandleReloadCanceled;
    }

    void UnsubscribeModelEvents(WeaponModel model)
    {
        if (model == null)
        {
            return;
        }

        model.ReloadStarted -= HandleReloadStarted;
        model.ReloadStepFilled -= HandleReloadStepFilled;
        model.ReloadCompleted -= HandleReloadCompleted;
        model.ReloadCanceled -= HandleReloadCanceled;
    }
#endregion

    void HandleReloadStarted() => PlayReloadSound();
    void HandleReloadStepFilled(int current, int max) { }
    void HandleReloadCompleted() { }
    void HandleReloadCanceled() { }

    public void SetPlayer(PlayerController playerController)
    {
        _playerController = playerController;
    }

    public void AimAtScreenPosition(Vector2 screenPosition)
    {
        if (_firePoint == null)
        {
            return;
        }

        var camera = _camera != null ? _camera : Camera.main;
        if (camera == null)
        {
            return;
        }

        var aimRay = camera.ScreenPointToRay(screenPosition);
        var aimPlane = new Plane(Vector3.up, new Vector3(0f, _firePoint.position.y, 0f));

        if (!aimPlane.Raycast(aimRay, out var enter))
        {
            return;
        }

        var target = aimRay.GetPoint(enter);

        var planarFromMuzzle = target - _firePoint.position;
        planarFromMuzzle.y = 0f;

        var minDistance = Mathf.Max(0f, _minAimDistance);
        var minSqrDistance = minDistance * minDistance;

        if (planarFromMuzzle.sqrMagnitude < Mathf.Max(minSqrDistance, 0.0001f))
        {
            return;
        }

        if (planarFromMuzzle.sqrMagnitude > 0.0001f)
        {
            var targetRotation = Quaternion.LookRotation(planarFromMuzzle.normalized, Vector3.up);
            var rotationSpeed = Mathf.Max(0f, _aimRotationSpeed);
            if (rotationSpeed <= 0f)
            {
                transform.rotation = targetRotation;
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);
            }
        }
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

        Model.StartReload();
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
        var baseDirection = _firePoint.forward;
        if (baseDirection.sqrMagnitude <= 0.0001f && _playerController != null)
        {
            baseDirection = _playerController.transform.forward;
        }
        var rayOrigin = spawnPosition;

        var bulletsPerShot = Mathf.Max(1, Model.Data.bulletsPerShot);
        var bulletSpeed = Model.BulletSpeed;
        var spreadAngle = Mathf.Max(0f, Model.Data.spreadAngle);
        var maxRange = Model.MaxRange;

        var damagePerBullet = Mathf.Max(0, Mathf.RoundToInt(Model.Data.damage));

        for (var i = 0; i < bulletsPerShot; i++)
        {
            var spreadDirection = ApplySpread(baseDirection, spreadAngle);
            var maxDistance = maxRange > 0f ? maxRange : Mathf.Infinity;
            var ray = new Ray(rayOrigin, spreadDirection);
            Vector3 targetPosition;
            IDamageable damageableTarget = null;

            Collider damageCollider = null;

            if (Physics.Raycast(ray, out var hitInfo, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
            {
                damageCollider = hitInfo.collider;
                targetPosition = hitInfo.point;
                damageableTarget = FindDamageableOnHit(damageCollider);
            }
            else
            {
                var fallbackDistance = maxRange > 0f ? maxRange : 1000f;
                targetPosition = spawnPosition + spreadDirection * fallbackDistance;
            }

            var bullet = pool.Get();
            bullet.Initialize(
                spawnPosition,
                targetPosition,
                bulletSpeed,
                damagePerBullet,
                damageableTarget,
                damageCollider);
        }
    }

    private static IDamageable FindDamageableOnHit(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return null;
        }

        if (hitCollider.TryGetComponent<DamageReceiver>(out var receiver) && receiver.TryGetDamageable(out var viaReceiver))
        {
            return viaReceiver;
        }

        return hitCollider.GetComponentInParent<IDamageable>();
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
