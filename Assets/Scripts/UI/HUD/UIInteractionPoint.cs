using UnityEngine;
using UnityEngine.UI;

public class UIInteractionPoint : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] Image ring;
    [SerializeField] GameObject interactiveKeyTip;
    [SerializeField, Tooltip("Scale when player reaches or exceeds focus radius.")] private float farScale = 1f;
    [SerializeField, Tooltip("Scale when player is within interact radius.")] private float nearScale = 0.6f;

    private RectTransform _rectTransform;
    private Camera _camera;
    private IInteractable _target;

    public IInteractable Target => _target;

    void Awake()
    {
        _rectTransform = transform as RectTransform;
        SetInteractiveKeyTipVisible(false);
    }

    public void Initialize(IInteractable target, Camera worldCamera)
    {
        _target = target;
        _camera = worldCamera != null ? worldCamera : Camera.main;
        SetVisible(true);
    }

    void LateUpdate()
    {
        if (_target == null || _camera == null || _rectTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 worldPos = _target.InteractionPoint != null
            ? _target.InteractionPoint.position
            : (_target is Component c ? c.transform.position : Vector3.zero);

        Vector3 screenPos = _camera.WorldToScreenPoint(worldPos);
        bool visible = screenPos.z > 0f;
        SetVisible(visible);

        if (!visible)
        {
            return;
        }

        _rectTransform.position = screenPos;
    }

    public void UpdateDistance(float distance)
    {
        if (ring == null || _target == null)
        {
            return;
        }

        float focusRadius = Mathf.Max(_target.FocusRadius, Mathf.Epsilon);
        float interactRadius = Mathf.Max(_target.InteractRadius, 0f);

        float clampedDistance = Mathf.Clamp(distance, interactRadius, focusRadius);
        float t = Mathf.Clamp01((clampedDistance - interactRadius) / Mathf.Max(focusRadius - interactRadius, Mathf.Epsilon));
        float scale = Mathf.Lerp(nearScale, farScale, t);
        ring.rectTransform.localScale = Vector3.one * scale;

        bool withinInteract = interactRadius <= 0f
            ? distance <= focusRadius
            : distance <= interactRadius;
        SetInteractiveKeyTipVisible(withinInteract);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
        }
        else
        {
            gameObject.SetActive(visible);
        }

        if (!visible)
        {
            SetInteractiveKeyTipVisible(false);
        }
    }

    private void SetInteractiveKeyTipVisible(bool visible)
    {
        if (interactiveKeyTip == null)
        {
            return;
        }

        if (interactiveKeyTip.activeSelf != visible)
        {
            interactiveKeyTip.SetActive(visible);
        }
    }
}
