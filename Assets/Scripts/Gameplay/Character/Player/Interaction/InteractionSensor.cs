using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pure logic helper that scans for nearby interactables based on physics overlap.
/// PlayerController should instantiate this and call Tick() each frame.
/// </summary>
public sealed class InteractionSensor
{
    private readonly PlayerController _owner;
    private readonly Transform _playerTransform;
    private readonly LayerMask _interactableMask;
    private readonly float _scanRadius;
    private readonly Collider[] _overlapBuffer;
    private readonly HashSet<IInteractable> _inRange = new();
    private readonly HashSet<IInteractable> _scanBuffer = new();
    private readonly List<IInteractable> _removalBuffer = new();

    public InteractionSensor(
        PlayerController owner,
        Transform playerTransform,
        float scanRadius,
        LayerMask interactableMask,
        int bufferSize = 32)
    {
        _owner = owner;
        _playerTransform = playerTransform;
        _scanRadius = Mathf.Max(0.1f, scanRadius);
        _interactableMask = interactableMask;
        _overlapBuffer = new Collider[Mathf.Max(1, bufferSize)];
    }

    public IInteractable Current { get; private set; }
    public float CurrentDistance { get; private set; }

    public event Action<IInteractable> CurrentChanged;
    public event Action<IInteractable, float> FocusDistanceChanged;
    public event Action<IInteractable> InteractableEntered;
    public event Action<IInteractable> InteractableExited;

    public void Tick()
    {
        ScanNearby();
        UpdateCurrent();

        if (_inRange.Count == 0)
        {
            CurrentDistance = 0f;
            return;
        }

        foreach (var candidate in _inRange)
        {
            if (candidate == null)
            {
                continue;
            }

            float distance = GetDistanceTo(candidate);
            if (candidate == Current)
            {
                CurrentDistance = distance;
            }

            FocusDistanceChanged?.Invoke(candidate, distance);
        }
    }

    public void Interact()
    {
        if (Current == null)
        {
            return;
        }

        CurrentDistance = GetDistanceTo(Current);
        if (CurrentDistance <= Current.InteractRadius)
        {
            Current.Interact(_owner);
        }
    }

    private void ScanNearby()
    {
        _scanBuffer.Clear();

        int count = Physics.OverlapSphereNonAlloc(
            _playerTransform.position,
            _scanRadius,
            _overlapBuffer,
            _interactableMask,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            var collider = _overlapBuffer[i];
            if (collider == null)
            {
                continue;
            }

            if (collider.TryGetComponent<IInteractable>(out var direct))
            {
                _scanBuffer.Add(direct);
                continue;
            }

            var interactable = collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                _scanBuffer.Add(interactable);
            }
        }

        foreach (var candidate in _scanBuffer)
        {
            if (candidate == null)
            {
                continue;
            }

            if (_inRange.Add(candidate))
            {
                InteractableEntered?.Invoke(candidate);
            }
        }

        _removalBuffer.Clear();
        foreach (var existing in _inRange)
        {
            if (!_scanBuffer.Contains(existing))
            {
                _removalBuffer.Add(existing);
            }
        }

        foreach (var removed in _removalBuffer)
        {
            _inRange.Remove(removed);
            if (removed == Current)
            {
                removed.EndFocus(_owner);
                Current = null;
                CurrentChanged?.Invoke(null);
            }
            InteractableExited?.Invoke(removed);
        }
    }

    private void UpdateCurrent()
    {
        IInteractable best = null;
        float bestDistance = float.MaxValue;

        foreach (var candidate in _inRange)
        {
            if (candidate == null)
            {
                continue;
            }

            float distance = GetDistanceTo(candidate);
            if (distance <= candidate.FocusRadius && distance < bestDistance)
            {
                best = candidate;
                bestDistance = distance;
            }
        }

        if (best != Current)
        {
            Current?.EndFocus(_owner);
            Current = best;
            CurrentDistance = best != null ? bestDistance : 0f;
            if (Current != null)
            {
                Current.BeginFocus(_owner);
            }
            CurrentChanged?.Invoke(Current);
        }
        else if (Current != null)
        {
            CurrentDistance = bestDistance;
        }
    }

    private float GetDistanceTo(IInteractable interactable)
    {
        var playerPos = _playerTransform.position;
        var interactionPoint = interactable.InteractionPoint != null
            ? interactable.InteractionPoint.position
            : Vector3.zero;

        return Vector3.Distance(playerPos, interactionPoint);
    }
}
