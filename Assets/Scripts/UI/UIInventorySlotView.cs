using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Displays a single inventory slot: icon and stack amount.
/// </summary>
public class UIInventorySlotView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;
    private int _index;
    private Action<int> _clickHandler;

    public void Initialize(int index, Action<int> clickHandler)
    {
        _index = index;
        _clickHandler = clickHandler;
    }

    public void SetSlot(InventoryModel.InventorySlot slot)
    {
        if (slot.IsEmpty)
        {
            if (icon != null)
            {
                icon.sprite = null;
                icon.enabled = false;
            }

            if (amountText != null)
            {
                amountText.gameObject.SetActive(false);
            }
            return;
        }

        if (icon != null)
        {
            icon.sprite = slot.Item?.Icon;
            icon.enabled = icon.sprite != null;
        }

        if (amountText != null)
        {
            bool showAmount = slot.Count > 1;
            amountText.gameObject.SetActive(showAmount);
            if (showAmount)
            {
                amountText.text = slot.Count.ToString();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _clickHandler?.Invoke(_index);
    }
}
