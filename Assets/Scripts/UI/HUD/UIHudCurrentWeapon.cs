using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHudCurrentWeapon : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI bulletText;
    [SerializeField] Image weaponIcon;
    [SerializeField] GameObject reloadFrame;
    [SerializeField] Image reloadProcessBar;
    private PlayerController player;
    private Weapon currentWeapon;
    private WeaponModel currentModel;
    private Coroutine attachRoutine;
    private bool isSubscribed;

    void Awake()
    {
        player = LevelManager.Instance.Player;
        if (reloadFrame != null)
        {
            reloadFrame.SetActive(false);
        }
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

        if (isSubscribed && player?.WeaponLoadoutController != null)
        {
            player.WeaponLoadoutController.WeaponActivated -= HandleWeaponActivated;
            player.WeaponLoadoutController.WeaponDeactivated -= HandleWeaponDeactivated;
            isSubscribed = false;
        }

        UnsubscribeCurrentWeapon();
        currentWeapon = null;
        currentModel = null;
        if (bulletText != null)
        {
            bulletText.text = "-/-";
        }

        if (weaponIcon != null)
        {
            weaponIcon.sprite = null;
            weaponIcon.enabled = false;
        }

        ResetReloadVisuals();
    }

    private void UpdateBulletText(int currentMagazineAmmo, int magazineCapacity)
    {
        if (bulletText == null)
        {
            return;
        }

        bulletText.text = $"{currentMagazineAmmo}/{magazineCapacity}";
    }

    private void OnReloadStart()
    {
        if (reloadFrame != null)
        {
            reloadFrame.SetActive(true);
        }

        if (reloadProcessBar != null)
        {
            reloadProcessBar.fillAmount = 0f;
        }
    }

    private void OnReloadComplet()
    {
        if (reloadFrame != null)
        {
            reloadFrame.SetActive(false);
        }

        if (reloadProcessBar != null)
        {
            reloadProcessBar.fillAmount = 0f;
        }
    }

    private void UpdateReloadProcess(float process)
    {
        if (reloadProcessBar != null)
        {
            reloadProcessBar.fillAmount = process;
        }
    }

    private IEnumerator EnsureBindings()
    {
        while (player == null)
        {
            player = LevelManager.Instance?.Player;
            yield return null;
        }

        while (player.WeaponLoadoutController == null)
        {
            yield return null;
        }

        if (!isSubscribed)
        {
            player.WeaponLoadoutController.WeaponActivated += HandleWeaponActivated;
            player.WeaponLoadoutController.WeaponDeactivated += HandleWeaponDeactivated;
            isSubscribed = true;
        }

        HandleWeaponActivated(player.WeaponLoadoutController.CurrentSlot);
        attachRoutine = null;
    }

    private void HandleWeaponActivated(WeaponLoadout.WeaponSlot slot)
    {
        if (slot.IsEmpty)
        {
            SetCurrentWeapon(null);
            return;
        }

        SetCurrentWeapon(slot.Instance);
    }

    private void HandleWeaponDeactivated(WeaponLoadout.WeaponSlot slot)
    {
        if (slot.IsEmpty || slot.Instance != currentWeapon)
        {
            return;
        }

        SetCurrentWeapon(null);
    }

    private void SetCurrentWeapon(Weapon newWeapon)
    {
        if (currentWeapon == newWeapon)
        {
            RefreshUiState();
            return;
        }

        UnsubscribeCurrentWeapon();
        currentWeapon = newWeapon;
        currentModel = currentWeapon != null ? currentWeapon.Model : null;

        if (currentModel != null)
        {
            currentModel.AmmoChanged += UpdateBulletText;
            currentModel.ReloadStarted += OnReloadStart;
            currentModel.ReloadCompleted += OnReloadComplet;
            currentModel.ReloadCanceled += OnReloadComplet;
            currentModel.ReloadProgressChanged += UpdateReloadProcess;
            currentModel.AmmoState();
            UpdateWeaponIcon();
            SyncReloadVisualsFromModel();
        }
        else
        {
            if (bulletText != null)
            {
                bulletText.text = "-/-";
            }

            UpdateWeaponIcon();
            ResetReloadVisuals();
        }
    }

    private void UnsubscribeCurrentWeapon()
    {
        if (currentModel == null)
        {
            return;
        }

        currentModel.AmmoChanged -= UpdateBulletText;
        currentModel.ReloadStarted -= OnReloadStart;
        currentModel.ReloadCompleted -= OnReloadComplet;
        currentModel.ReloadCanceled -= OnReloadComplet;
        currentModel.ReloadProgressChanged -= UpdateReloadProcess;
    }

    private void RefreshUiState()
    {
        if (currentModel != null)
        {
            currentModel.AmmoState();
            UpdateWeaponIcon();
            SyncReloadVisualsFromModel();
        }
    }

    private void UpdateWeaponIcon()
    {
        if (weaponIcon == null)
        {
            return;
        }

        var sprite = currentModel != null ? currentModel.Data?.sprite : null;
        weaponIcon.sprite = sprite;
        weaponIcon.enabled = sprite != null;
    }

    private void ResetReloadVisuals()
    {
        if (reloadFrame != null)
        {
            reloadFrame.SetActive(false);
        }

        if (reloadProcessBar != null)
        {
            reloadProcessBar.fillAmount = 0f;
        }
    }

    private void SyncReloadVisualsFromModel()
    {
        if (currentModel == null)
        {
            ResetReloadVisuals();
            return;
        }

        if (currentModel.IsReloading)
        {
            if (reloadFrame != null)
            {
                reloadFrame.SetActive(true);
            }

            if (reloadProcessBar != null)
            {
                reloadProcessBar.fillAmount = currentModel.ReloadProgress;
            }
        }
        else
        {
            ResetReloadVisuals();
        }
    }
}
