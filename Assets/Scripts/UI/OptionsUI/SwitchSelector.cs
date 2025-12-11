using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwitchSelector : MonoBehaviour
{
    [SerializeField] Button leftButton;
    [SerializeField] Button rightButton;
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] String[] options;
    private int currentIndex;

    void OnEnable()
    {
        if (leftButton != null)
        {
            leftButton.onClick.AddListener(HandleLeft);
        }

        if (rightButton != null)
        {
            rightButton.onClick.AddListener(HandleRight);
        }

        RefreshLabel();
    }

    void OnDisable()
    {
        if (leftButton != null)
        {
            leftButton.onClick.RemoveListener(HandleLeft);
        }

        if (rightButton != null)
        {
            rightButton.onClick.RemoveListener(HandleRight);
        }
    }

    private void HandleLeft()
    {
        ShiftIndex(-1);
    }

    private void HandleRight()
    {
        ShiftIndex(1);
    }

    private void ShiftIndex(int delta)
    {
        int count = options != null ? options.Length : 0;
        if (count <= 0)
        {
            currentIndex = 0;
            RefreshLabel();
            return;
        }

        currentIndex = (currentIndex + delta) % count;
        if (currentIndex < 0)
        {
            currentIndex += count;
        }

        RefreshLabel();
    }

    private void RefreshLabel()
    {
        if (label == null)
        {
            return;
        }

        if (options == null || options.Length == 0)
        {
            label.text = string.Empty;
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, options.Length - 1);
        label.text = options[currentIndex];
    }
}
