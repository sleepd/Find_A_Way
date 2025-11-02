using System;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Generic helper that wraps Unity's <see cref="ObjectPool{T}"/> for components implementing <see cref="IPoolable{T}"/>.
/// </summary>
/// <typeparam name="T">Component type to pool.</typeparam>
public sealed class ComponentPool<T> where T : Component, IPoolable<T>
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly bool _collectionCheck;
    private readonly ObjectPool<T> _pool;

    /// <summary>
    /// Creates a pool for the given prefab.
    /// </summary>
    /// <param name="prefab">Prefab instance that will be cloned for new pooled objects.</param>
    /// <param name="parent">Optional parent transform for instantiated objects.</param>
    /// <param name="defaultCapacity">Initial size of the pool.</param>
    /// <param name="maxSize">Maximum number of objects the pool is allowed to hold.</param>
    /// <param name="collectionCheck">
    /// When true, Unity's pool will throw if an object is released twice. Disable to skip the check for performance.
    /// </param>
    public ComponentPool(
        T prefab,
        Transform parent = null,
        int defaultCapacity = 10,
        int maxSize = 100,
        bool collectionCheck = true)
    {
        _prefab = prefab != null ? prefab : throw new ArgumentNullException(nameof(prefab));
        _parent = parent;
        _collectionCheck = collectionCheck;

        _pool = new ObjectPool<T>(
            CreateItem,
            OnTakeFromPool,
            OnReturnToPool,
            OnDestroyPoolObject,
            collectionCheck,
            Mathf.Max(0, defaultCapacity),
            Mathf.Max(1, maxSize));
    }

    /// <summary>
    /// Retrieves an instance from the pool.
    /// </summary>
    public T Get() => _pool.Get();

    /// <summary>
    /// Returns an instance back to the pool.
    /// </summary>
    public void Release(T instance)
    {
        if (instance == null)
        {
            return;
        }

        _pool.Release(instance);
    }

    /// <summary>
    /// Disposes of all inactive instances in the pool.
    /// </summary>
    public void Clear() => _pool.Clear();

    /// <summary>
    /// Number of inactive instances currently stored.
    /// </summary>
    public int InactiveCount => _pool.CountInactive;

    private T CreateItem()
    {
        var instance = UnityEngine.Object.Instantiate(_prefab, _parent);
        instance.SetPool(_pool);
        return instance;
    }

    private void OnTakeFromPool(T instance)
    {
        if (instance == null)
        {
            return;
        }

        instance.OnTakenFromPool();
    }

    private void OnReturnToPool(T instance)
    {
        if (instance == null)
        {
            return;
        }

        instance.OnReturnedToPool();
    }

    private void OnDestroyPoolObject(T instance)
    {
        if (instance == null)
        {
            return;
        }

        UnityEngine.Object.Destroy(instance.gameObject);
    }
}
