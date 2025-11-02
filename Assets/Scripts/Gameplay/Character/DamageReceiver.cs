using UnityEngine;

/// <summary>
/// Helper component that sits on hitbox colliders and resolves the owning IDamageable.
/// </summary>
[DisallowMultipleComponent]
public class DamageReceiver : MonoBehaviour
{
    [SerializeField, Tooltip("Optional explicit reference to the component that implements IDamageable.")]
    private MonoBehaviour damageableProvider;

    private IDamageable cachedDamageable;

    public bool TryGetDamageable(out IDamageable damageable)
    {
        damageable = ResolveDamageable();
        return damageable != null;
    }

    public IDamageable ResolveDamageable()
    {
        if (cachedDamageable != null)
        {
            return cachedDamageable;
        }

        if (damageableProvider != null)
        {
            cachedDamageable = damageableProvider as IDamageable;
            if (cachedDamageable == null)
            {
                Debug.LogError($"{damageableProvider.name} does not implement IDamageable but was assigned as provider.", this);
            }
        }

        if (cachedDamageable == null)
        {
            cachedDamageable = GetComponentInParent<IDamageable>();
        }

        return cachedDamageable;
    }
}
