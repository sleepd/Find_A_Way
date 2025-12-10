using System;
using UnityEngine;

/// <summary>
/// Runtime health model for a character. Keeps track of current/max health,
/// supports damage, healing, invulnerability, and raises helpful events.
/// </summary>
public class CharacterHealth : IDamageable
{
    private int currentHealth;

    public CharacterHealth(int maxHealth)
        : this(maxHealth, maxHealth)
    {
    }

    public CharacterHealth(int maxHealth, int startingHealth, bool isInvincible = false)
    {
        if (maxHealth <= 0)
        {
            Debug.LogError("Max health must be greater than zero. Falling back to 1.");
            maxHealth = 1;
        }

        MaxHealth = maxHealth;
        currentHealth = Mathf.Clamp(startingHealth, 0, MaxHealth);
        IsInvincible = isInvincible;
        HealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    public int MaxHealth { get; private set; }
    public int CurrentHealth => currentHealth;
    public float NormalizedHealth => MaxHealth > 0 ? currentHealth / (float)MaxHealth : 0f;
    public bool IsDead => currentHealth <= 0;
    public bool IsInvincible { get; private set; }

    public event Action<int, int> HealthChanged;
    public event Action<int, int> Damaged;
    public event Action<int, int> Healed;
    public event Action Died;
    public event Action Revived;

    public void Reset(int maxHealth)
    {
        if (maxHealth <= 0)
        {
            Debug.LogError("Max health must be greater than zero. Reset aborted.");
            return;
        }

        bool wasDead = IsDead;
        MaxHealth = maxHealth;
        currentHealth = MaxHealth;
        HealthChanged?.Invoke(currentHealth, MaxHealth);

        if (wasDead && !IsDead)
        {
            Revived?.Invoke();
        }
    }

    public void SetInvincible(bool isInvincible)
    {
        IsInvincible = isInvincible;
    }

    public void SetMaxHealth(int newMaxHealth, bool preserveRatio = true)
    {
        if (newMaxHealth <= 0)
        {
            Debug.LogError("Max health must be greater than zero. SetMaxHealth aborted.");
            return;
        }

        if (newMaxHealth == MaxHealth)
        {
            return;
        }

        int previousMax = MaxHealth;
        int previousHealth = currentHealth;
        MaxHealth = newMaxHealth;

        if (preserveRatio && previousMax > 0)
        {
            float ratio = previousHealth / (float)previousMax;
            currentHealth = Mathf.Clamp(Mathf.RoundToInt(ratio * MaxHealth), 0, MaxHealth);
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
        }

        if (previousHealth != currentHealth || previousMax != MaxHealth)
        {
            HealthChanged?.Invoke(currentHealth, MaxHealth);
        }
    }

    public int ApplyDamage(int amount)
    {
        if (amount <= 0 || IsDead || IsInvincible)
        {
            return 0;
        }

        int previousHealth = currentHealth;
        int actualDamage = Mathf.Min(amount, currentHealth);
        currentHealth -= actualDamage;

        Damaged?.Invoke(actualDamage, currentHealth);
        HealthChanged?.Invoke(currentHealth, MaxHealth);

        if (previousHealth > 0 && IsDead)
        {
            Debug.Log("[CharacterHealth] Died event invoked.");
            Died?.Invoke();
        }

        return actualDamage;
    }

    public int Heal(int amount, bool allowRevive = false)
    {
        if (amount <= 0)
        {
            return 0;
        }

        bool wasDead = IsDead;
        if (wasDead && !allowRevive)
        {
            return 0;
        }

        int previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, MaxHealth);
        int actualHealed = currentHealth - previousHealth;

        if (actualHealed <= 0)
        {
            return 0;
        }

        Healed?.Invoke(actualHealed, currentHealth);
        HealthChanged?.Invoke(currentHealth, MaxHealth);

        if (wasDead && currentHealth > 0)
        {
            Revived?.Invoke();
        }

        return actualHealed;
    }

    public void Kill()
    {
        if (IsDead)
        {
            return;
        }

        currentHealth = 0;
        HealthChanged?.Invoke(currentHealth, MaxHealth);
        Debug.Log("[CharacterHealth] Kill called, Died event invoked.");
        Died?.Invoke();
    }

    public int RestoreFullHealth(bool allowRevive = true)
    {
        if (MaxHealth <= 0)
        {
            return 0;
        }

        bool wasDead = IsDead;
        if (wasDead && !allowRevive)
        {
            return 0;
        }

        int previousHealth = currentHealth;
        currentHealth = MaxHealth;
        int amountHealed = currentHealth - previousHealth;

        if (amountHealed == 0)
        {
            return 0;
        }

        Healed?.Invoke(amountHealed, currentHealth);
        HealthChanged?.Invoke(currentHealth, MaxHealth);

        if (wasDead && currentHealth > 0)
        {
            Revived?.Invoke();
        }

        return amountHealed;
    }
}
