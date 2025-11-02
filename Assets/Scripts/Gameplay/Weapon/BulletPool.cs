using UnityEngine;

public class BulletPool : Singleton<BulletPool>
{
    [SerializeField] private Bullet bulletPrefab;
    private ComponentPool<Bullet> _bulletPool;

    public ComponentPool<Bullet> Pool => _bulletPool;

    public override void Awake()
    {
        base.Awake();
        _bulletPool = new ComponentPool<Bullet>(bulletPrefab, transform, defaultCapacity: 20, maxSize: 100);
    }
    
    public Bullet Get()
    {
        var bullet = _bulletPool.Get();
        return bullet;
    }

    public void Release(Bullet bullet)
    {
        if (bullet == null || _bulletPool == null)
        {
            return;
        }

        _bulletPool.Release(bullet);
    }
}
