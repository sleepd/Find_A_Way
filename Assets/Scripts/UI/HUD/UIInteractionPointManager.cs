using System.Collections.Generic;
using UnityEngine;

public class UIInteractionPointManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private UIInteractionPoint pointPrefab;
    [SerializeField] private RectTransform container;
    [SerializeField] private Camera worldCamera;

    private InteractionSensor _sensor;
    private Transform _playerTransform;
    private readonly Dictionary<IInteractable, UIInteractionPoint> _points = new();

    void Awake()
    {
        if (container == null)
        {
            container = transform as RectTransform;
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }
    }

    void OnEnable()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        _sensor = playerController?.InteractionSensor;
        if (_sensor == null)
        {
            Debug.LogWarning("UIInteractionPointManager could not find InteractionSensor.", this);
            return;
        }
        _playerTransform = playerController.transform;

        _sensor.InteractableEntered += HandleInteractableEntered;
        _sensor.InteractableExited += HandleInteractableExited;
        _sensor.CurrentChanged += HandleCurrentChanged;
        _sensor.FocusDistanceChanged += HandleFocusDistanceChanged;
    }

    void OnDisable()
    {
        if (_sensor != null)
        {
            _sensor.InteractableEntered -= HandleInteractableEntered;
            _sensor.InteractableExited -= HandleInteractableExited;
            _sensor.CurrentChanged -= HandleCurrentChanged;
            _sensor.FocusDistanceChanged -= HandleFocusDistanceChanged;
        }

        foreach (var point in _points.Values)
        {
            if (point != null)
            {
                Destroy(point.gameObject);
            }
        }

        _points.Clear();
        _sensor = null;
    }

    private void HandleInteractableEntered(IInteractable interactable)
    {
        if (interactable == null || _points.ContainsKey(interactable) || pointPrefab == null || container == null)
        {
            return;
        }

        var instance = Instantiate(pointPrefab, container);
        instance.Initialize(interactable, worldCamera);
        UpdatePointDistance(instance, interactable);
        _points.Add(interactable, instance);
    }

    private void HandleInteractableExited(IInteractable interactable)
    {
        if (interactable == null)
        {
            return;
        }

        if (_points.TryGetValue(interactable, out var point))
        {
            if (point != null)
            {
                Destroy(point.gameObject);
            }

            _points.Remove(interactable);
        }
    }

    private void HandleCurrentChanged(IInteractable current)
    {
        // Points all look the same; nothing to update.
    }

    private void HandleFocusDistanceChanged(IInteractable interactable, float distance)
    {
        if (interactable == null)
        {
            return;
        }

        if (_points.TryGetValue(interactable, out var point))
        {
            point.UpdateDistance(distance);
        }
    }

    private void UpdatePointDistance(UIInteractionPoint point, IInteractable interactable)
    {
        if (point == null || interactable == null || _playerTransform == null)
        {
            return;
        }

        Vector3 targetPos = interactable.InteractionPoint != null
            ? interactable.InteractionPoint.position
            : (interactable is Component c ? c.transform.position : Vector3.zero);

        float distance = Vector3.Distance(_playerTransform.position, targetPos);
        point.UpdateDistance(distance);
    }
}
