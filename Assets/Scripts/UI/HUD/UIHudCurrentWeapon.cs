using System;
using TMPro;
using UnityEngine;

public class UIHudCurrentWeapon : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _bulletText;
    private PlayerController _player;

    void Awake()
    {
        _player = LevelManager.Instance.Player;
    }

    void OnEnable()
    {
        _player.CurrentWeapon.Model.AmmoChanged += UpdateBulletText;
        _player.CurrentWeapon.Model.AmmoState();
    }

    void OnDisable()
    {
        _player.CurrentWeapon.Model.AmmoChanged -= UpdateBulletText;
    }

    private void UpdateBulletText(int currentMagazineAmmo, int magazineCapacity)
    {
        if (_bulletText == null)
        {
            return;
        }

        _bulletText.text = $"{currentMagazineAmmo}/{magazineCapacity}";
    }
}
