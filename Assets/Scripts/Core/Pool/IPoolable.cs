using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Base contract for objects that participate in pooling.
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// Called when the object is taken from the pool and becomes active.
    /// </summary>
    void OnTakenFromPool();

    /// <summary>
    /// Called when the object is returned to the pool and should reset its state.
    /// </summary>
    void OnReturnedToPool();

    /// <summary>
    /// Actively release the object back to its pool when finished.
    /// </summary>
    void Despawn();
}

/// <summary>
/// Contract for Unity components that are managed by an <see cref="IObjectPool{T}"/>.
/// </summary>
/// <typeparam name="T">Component type that is pooled.</typeparam>
public interface IPoolable<T> : IPoolable where T : Component
{
    /// <summary>
    /// Provides the backing pool so the instance can release itself when finished.
    /// </summary>
    /// <param name="pool">The pool that owns this instance.</param>
    void SetPool(IObjectPool<T> pool);
}
