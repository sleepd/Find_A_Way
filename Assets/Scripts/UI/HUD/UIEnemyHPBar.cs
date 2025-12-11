using UnityEngine;
using UnityEngine.UI;

public class UIEnemyHPBar : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] Image hpbar;
    private CharacterHealth health;
    private Canvas canvas;
    private bool subscribed;
    private Coroutine bindRoutine;
    private EnemyController enemy;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        enemy = transform.parent != null ? transform.parent.GetComponent<EnemyController>() : null;
    }

    void OnEnable()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (canvas != null)
        {
            canvas.worldCamera = targetCamera;
        }

        if (bindRoutine != null)
        {
            StopCoroutine(bindRoutine);
        }
        bindRoutine = StartCoroutine(BindWhenReady());
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (canvas != null)
            {
                canvas.worldCamera = targetCamera;
            }
        }

        if (targetCamera == null)
        {
            return;
        }

        // Absolute billboard: face camera direction (pitch/yaw), ignoring parent rotation.
        Vector3 toCamera = targetCamera.transform.position - transform.position;
        if (toCamera.sqrMagnitude < 0.0001f)
        {
            toCamera = targetCamera.transform.forward;
        }

        transform.rotation = Quaternion.LookRotation(toCamera, Vector3.up);
    }

    void OnDisable()
    {
        Unsubscribe();
        if (bindRoutine != null)
        {
            StopCoroutine(bindRoutine);
            bindRoutine = null;
        }
    }

    private void Subscribe()
    {
        if (subscribed || health == null)
        {
            return;
        }

        health.HealthChanged += HandleHealthChanged;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed || health == null)
        {
            return;
        }

        health.HealthChanged -= HandleHealthChanged;
        subscribed = false;
    }

    private void HandleHealthChanged(int current, int max)
    {
        UpdateFill(current, max);
    }

    private void SyncFill()
    {
        UpdateFill(health.CurrentHealth, health.MaxHealth);
    }

    private void UpdateFill(int current, int max)
    {
        if (hpbar == null)
        {
            return;
        }

        float fill = max > 0 ? Mathf.Clamp01(current / (float)max) : 0f;
        hpbar.fillAmount = fill;
    }

    private System.Collections.IEnumerator BindWhenReady()
    {
        while (health == null)
        {
            if (enemy == null && transform.parent != null)
            {
                enemy = transform.parent.GetComponent<EnemyController>();
            }

            if (enemy != null)
            {
                health = enemy.Health;
            }

            if (health == null)
            {
                yield return null;
                continue;
            }
        }

        Subscribe();
        SyncFill();
        bindRoutine = null;
    }
}
