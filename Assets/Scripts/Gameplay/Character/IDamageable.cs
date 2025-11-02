using System;

/// <summary>
/// Contract for objects that can receive damage and healing,
/// exposing health state, events, and basic control operations.
/// </summary>
public interface IDamageable
{
    int MaxHealth { get; }
    int CurrentHealth { get; }
    float NormalizedHealth { get; }
    bool IsDead { get; }
    bool IsInvincible { get; }

    event Action<int, int> HealthChanged;
    event Action<int, int> Damaged;
    event Action<int, int> Healed;
    event Action Died;
    event Action Revived;

    int ApplyDamage(int amount);
    int Heal(int amount, bool allowRevive = false);
    int RestoreFullHealth(bool allowRevive = true);
    void Kill();
    void SetInvincible(bool isInvincible);
}
