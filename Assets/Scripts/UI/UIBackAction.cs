using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Mimics a UI button that listens for the EventSystem's configured input
/// and triggers a cancel/submit style action plus optional callbacks.
/// </summary>
public class UIBackAction : MonoBehaviour
{
    private enum TriggerButton
    {
        Submit,
        Cancel,
        Custom
    }

    [Header("Input")]
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private TriggerButton triggerButton = TriggerButton.Cancel;
    [SerializeField] private string customInputName = string.Empty;

    [Header("Target")]
    [SerializeField] private GameObject target;
    [SerializeField] private string methodName = string.Empty;

    [Header("Events")]
    [SerializeField] private UnityEvent onTriggered;

    private void Awake()
    {
        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
        }
    }

    private void Update()
    {
        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
        }

        if (!IsTriggeredThisFrame())
        {
            return;
        }

        Trigger();
    }

    private bool IsTriggeredThisFrame()
    {
        if (eventSystem == null || eventSystem.currentInputModule is not InputSystemUIInputModule inputSystemModule)
        {
            return false;
        }

        var action = ResolveInputAction(inputSystemModule);
        return action != null && action.enabled && action.WasPerformedThisFrame();
    }

    private InputAction ResolveInputAction(InputSystemUIInputModule module)
    {
        return triggerButton switch
        {
            TriggerButton.Submit => module.submit.action,
            TriggerButton.Cancel => module.cancel.action,
            TriggerButton.Custom => ResolveCustomAction(module),
            _ => ResolveCustomAction(module)
        };
    }

    private InputAction ResolveCustomAction(InputSystemUIInputModule module)
    {
        var name = GetCustomInputName();
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        var actionAsset = module.actionsAsset;
        return actionAsset == null ? null : actionAsset.FindAction(name, false);
    }

    private string GetCustomInputName()
    {
        return string.IsNullOrWhiteSpace(customInputName) ? null : customInputName;
    }

    private void Trigger()
    {
        DispatchUIEvent();

        if (target != null && !string.IsNullOrWhiteSpace(methodName))
        {
            target.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
        }

        onTriggered?.Invoke();
    }

    private void DispatchUIEvent()
    {
        if (eventSystem == null)
        {
            return;
        }

        var current = eventSystem.currentSelectedGameObject;
        if (current == null)
        {
            return;
        }

        var eventData = new BaseEventData(eventSystem);
        switch (triggerButton)
        {
            case TriggerButton.Submit:
                ExecuteEvents.Execute(current, eventData, ExecuteEvents.submitHandler);
                break;
            case TriggerButton.Cancel:
                ExecuteEvents.Execute(current, eventData, ExecuteEvents.cancelHandler);
                break;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
        }
    }
#endif
}
