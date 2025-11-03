using UnityEngine;

/// <summary>
/// Generic interaction contract. Anything that a player can focus/activate should implement this.
/// </summary>
public interface IInteractable
{
    /// <summary>World position used for distance checks and UI indicators.</summary>
    Transform InteractionPoint { get; }

    /// <summary>Distance threshold at which the object starts showing focus hints.</summary>
    float FocusRadius { get; }

    /// <summary>Distance threshold at which the player can trigger the interaction.</summary>
    float InteractRadius { get; }

    /// <summary>Short label to display in the UI (e.g., "Open Chest").</summary>
    string DisplayName { get; }

    /// <summary>
    /// Called when a player enters focus range. Ideal for highlighting the object or showing indicators.
    /// </summary>
    void BeginFocus(PlayerController player);

    /// <summary>
    /// Called when a player leaves focus range.
    /// </summary>
    void EndFocus(PlayerController player);

    /// <summary>
    /// Executes the interaction (e.g., opening a container). Should be called when player presses the interact key.
    /// </summary>
    void Interact(PlayerController player);
}
