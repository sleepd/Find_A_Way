using System;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1)] private int maxHealth = 10;

    public CharacterHealth Health { get; private set; }

    public int MaxHealth => Health?.MaxHealth ?? Mathf.Max(1, maxHealth);
    public int CurrentHealth => Health?.CurrentHealth ?? Mathf.Max(0, maxHealth);
    public float NormalizedHealth => Health != null ? Health.NormalizedHealth : 1f;
    public bool IsDead => Health?.IsDead ?? false;
    public bool IsInvincible => Health?.IsInvincible ?? false;

    public event Action<int, int> HealthChanged;
    public event Action<int, int> Damaged;
    public event Action<int, int> Healed;
    public event Action Died;
    public event Action Revived;

    void Awake()
    {
        Health = new CharacterHealth(Mathf.Max(1, maxHealth));
        SubscribeToHealthEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromHealthEvents();
    }

    public int ApplyDamage(int amount) => Health != null ? Health.ApplyDamage(amount) : 0;

    public int Heal(int amount, bool allowRevive = false) =>
        Health != null ? Health.Heal(amount, allowRevive) : 0;

    public int RestoreFullHealth(bool allowRevive = true) =>
        Health != null ? Health.RestoreFullHealth(allowRevive) : 0;

    public void Kill()
    {
        if (Health != null)
        {
            Health.Kill();
        }
    }

    public void SetInvincible(bool isInvincible)
    {
        if (Health != null)
        {
            Health.SetInvincible(isInvincible);
        }
    }

    void OnValidate()
    {
        if (maxHealth < 1)
        {
            maxHealth = 1;
        }
    }

    private void SubscribeToHealthEvents()
    {
        if (Health == null)
        {
            return;
        }

        Health.HealthChanged += HandleHealthChanged;
        Health.Damaged += HandleDamaged;
        Health.Healed += HandleHealed;
        Health.Died += HandleDied;
        Health.Revived += HandleRevived;
    }

    private void UnsubscribeFromHealthEvents()
    {
        if (Health == null)
        {
            return;
        }

        Health.HealthChanged -= HandleHealthChanged;
        Health.Damaged -= HandleDamaged;
        Health.Healed -= HandleHealed;
        Health.Died -= HandleDied;
        Health.Revived -= HandleRevived;
    }

    private void HandleHealthChanged(int current, int max) => HealthChanged?.Invoke(current, max);
    private void HandleDamaged(int amount, int remaining) => Damaged?.Invoke(amount, remaining);
    private void HandleHealed(int amount, int current) => Healed?.Invoke(amount, current);
    private void HandleDied()
    {
        Died?.Invoke();
        // Temporary destory
        Destroy(gameObject);
    } 
    private void HandleRevived() => Revived?.Invoke();
}
