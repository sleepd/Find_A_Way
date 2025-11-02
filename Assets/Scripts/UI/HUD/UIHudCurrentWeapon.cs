using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHudCurrentWeapon : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _bulletText;
    [SerializeField] GameObject _reloadFrame;
    [SerializeField] Image _reloadProcessBar;
    private PlayerController _player;

    void Awake()
    {
        _player = LevelManager.Instance.Player;
        _reloadFrame.SetActive(false);
    }

    void OnEnable()
    {
        _player.CurrentWeapon.Model.AmmoChanged += UpdateBulletText;
        _player.CurrentWeapon.Model.ReloadStarted += OnReloadStart;
        _player.CurrentWeapon.Model.ReloadCompleted += OnReloadComplet;
        _player.CurrentWeapon.Model.ReloadCanceled += OnReloadComplet;
        _player.CurrentWeapon.Model.ReloadProgressChanged += UpdateReloadProcess;
        _player.CurrentWeapon.Model.AmmoState();
    }

    void OnDisable()
    {
        _player.CurrentWeapon.Model.AmmoChanged -= UpdateBulletText;
        _player.CurrentWeapon.Model.ReloadStarted -= OnReloadStart;
        _player.CurrentWeapon.Model.ReloadCompleted -= OnReloadComplet;
        _player.CurrentWeapon.Model.ReloadCanceled -= OnReloadComplet;
    }

    private void UpdateBulletText(int currentMagazineAmmo, int magazineCapacity)
    {
        if (_bulletText == null)
        {
            return;
        }

        _bulletText.text = $"{currentMagazineAmmo}/{magazineCapacity}";
    }

    private void OnReloadStart()
    {
        _reloadFrame.SetActive(true);
        _reloadProcessBar.fillAmount = 0;
    }

    private void OnReloadComplet()
    {
        _reloadFrame.SetActive(false);
        _reloadProcessBar.fillAmount = 0;
    }

    private void UpdateReloadProcess(float process)
    {
        _reloadProcessBar.fillAmount = process;
    }


}
