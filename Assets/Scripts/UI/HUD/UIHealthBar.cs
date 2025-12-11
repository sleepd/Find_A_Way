using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField] Image healthBar;
    [Header("Colors")]
    [SerializeField] Color normalColor = Color.green;
    [SerializeField] Color halfColor = Color.yellow;
    [SerializeField] Color lowColor = Color.red;

    [Tooltip("0-1 normalized threshold for switching to lowColor.")]
    [SerializeField, Range(0f, 1f)] float lowThreshold = 0.25f;
    [Tooltip("0-1 normalized threshold for switching to halfColor.")]
    [SerializeField, Range(0f, 1f)] float halfThreshold = 0.5f;

    private PlayerController player;
    private bool isSubscribed;
    private Coroutine attachRoutine;

    void Awake()
    {
        player = LevelManager.Instance != null ? LevelManager.Instance.Player : null;
    }

    void OnEnable()
    {
        attachRoutine = StartCoroutine(EnsureBindings());
    }

    void OnDisable()
    {
        if (attachRoutine != null)
        {
            StopCoroutine(attachRoutine);
            attachRoutine = null;
        }

        Unsubscribe();
    }

    private IEnumerator EnsureBindings()
    {
        while (player == null)
        {
            player = LevelManager.Instance != null ? LevelManager.Instance.Player : null;
            yield return null;
        }

        Subscribe();
        SyncFromPlayer();
        attachRoutine = null;
    }

    private void Subscribe()
    {
        if (isSubscribed || player?.Health == null)
        {
            return;
        }

        player.Health.HealthChanged += HandleHealthChanged;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || player?.Health == null)
        {
            return;
        }

        player.Health.HealthChanged -= HandleHealthChanged;
        isSubscribed = false;
    }

    private void SyncFromPlayer()
    {
        if (player?.Health == null)
        {
            UpdateFill(0, 1);
            return;
        }

        UpdateFill(player.Health.CurrentHealth, player.Health.MaxHealth);
    }

    private void HandleHealthChanged(int current, int max)
    {
        UpdateFill(current, max);
    }

    private void UpdateFill(int current, int max)
    {
        if (healthBar == null)
        {
            return;
        }

        float fill = max > 0 ? Mathf.Clamp01(current / (float)max) : 0f;
        healthBar.fillAmount = fill;
        healthBar.color = ChooseColor(fill);
    }

    private Color ChooseColor(float fill)
    {
        // low -> half -> normal ordering
        if (fill <= lowThreshold)
        {
            return lowColor;
        }
        if (fill <= halfThreshold)
        {
            return halfColor;
        }

        return normalColor;
    }
}
