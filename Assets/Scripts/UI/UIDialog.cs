using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class UIDialog : MonoBehaviour
{
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _contentText;

    private UnityAction _onConfirm;
    private UnityAction _onClose;

    private void OnEnable()
    {
        if (_cancelButton != null)
        {
            _cancelButton.onClick.AddListener(OnCancelPressed);
            _cancelButton.Select();
        }

        if (_confirmButton != null && _onConfirm != null)
        {
            _confirmButton.onClick.AddListener(OnConfirmClicked);
        }
    }

    private void OnDisable()
    {
        if (_cancelButton != null)
        {
            _cancelButton.onClick.RemoveListener(OnCancelPressed);
        }

        if (_confirmButton != null)
        {
            _confirmButton.onClick.RemoveListener(OnConfirmClicked);
        }
    }

    public void Initialize(string title, string content, UnityAction confirmCallback, UnityAction closeCallback)
    {
        if (_titleText != null)
        {
            _titleText.text = title ?? string.Empty;
        }

        if (_contentText != null)
        {
            _contentText.text = content ?? string.Empty;
        }

        SetConfirmCallback(confirmCallback);
        SetCloseCallback(closeCallback);
    }

    private void SetConfirmCallback(UnityAction callback)
    {
        _onConfirm = callback;

        if (_confirmButton == null)
        {
            return;
        }

        _confirmButton.onClick.RemoveListener(OnConfirmClicked);

        if (_onConfirm != null)
        {
            _confirmButton.onClick.AddListener(OnConfirmClicked);
        }
    }

    private void SetCloseCallback(UnityAction callback)
    {
        _onClose = callback;
    }

    private void OnCancelPressed()
    {
        Close();
    }

    private void Close()
    {
        _onClose?.Invoke();
        ClearCallbacks();
        gameObject.SetActive(false);
    }

    private void ClearCallbacks()
    {
        if (_confirmButton != null)
        {
            _confirmButton.onClick.RemoveListener(OnConfirmClicked);
        }

        if (_cancelButton != null)
        {
            _cancelButton.onClick.RemoveListener(OnCancelPressed);
        }

        _onConfirm = null;
        _onClose = null;
    }

    private void OnConfirmClicked()
    {
        _onConfirm?.Invoke();
    }
}
