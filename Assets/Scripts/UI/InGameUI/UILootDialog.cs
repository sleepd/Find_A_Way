using UnityEngine;

public class UILootDialog : MonoBehaviour
{
    [SerializeField] GameObject lootsContainerPanel;
    [SerializeField] UIInventoryView lootInventoryView;
    [SerializeField] UIInGameMenu inGameMenu;
    private ItemContainer _currentContainer;

    public ItemContainer CurrentContainer => _currentContainer;
    public event System.Action<ItemContainer> Shown;
    public event System.Action<ItemContainer> Hidden;

    void Awake()
    {
        if (lootsContainerPanel != null)
        {
            lootsContainerPanel.SetActive(false);
        }
    }

    public void Show(ItemContainer container)
    {
        _currentContainer = container;
        if (lootInventoryView != null && container != null)
        {
            lootInventoryView.SetOnSlotClicked(idx => HandleLootSlotClicked(idx));
            lootInventoryView.Bind(container.Model);
        }

        if (lootsContainerPanel != null)
        {
            lootsContainerPanel.SetActive(true);
        }

        if (container != null)
        {
            Shown?.Invoke(container);
        }
    }

    public void Hide()
    {
        var closingContainer = _currentContainer;
        _currentContainer = null;
        if (lootsContainerPanel != null)
        {
            lootsContainerPanel.SetActive(false);
        }

        if (closingContainer != null)
        {
            Hidden?.Invoke(closingContainer);
        }
    }

    private void HandleLootSlotClicked(int slotIndex)
    {
        if (_currentContainer == null || LevelManager.Instance == null || LevelManager.Instance.Player == null)
        {
            return;
        }

        var playerInventory = LevelManager.Instance.Player.Inventory;
        if (playerInventory == null)
        {
            return;
        }

        if (_currentContainer.TryTakeFromSlot(slotIndex, int.MaxValue, out var item, out var removed) && removed > 0)
        {
            playerInventory.AddItem(item, removed);
        }
    }

    void Update()
    {
        if (_currentContainer == null)
        {
            return;
        }

        var player = LevelManager.Instance != null ? LevelManager.Instance.Player : null;
        if (player == null)
        {
            return;
        }

        var point = _currentContainer.InteractionPoint != null
            ? _currentContainer.InteractionPoint.position
            : _currentContainer.transform.position;
        float dist = Vector3.Distance(player.transform.position, point);
        if (dist > _currentContainer.InteractRadius)
        {
            Hide();
            var menu = inGameMenu != null
                ? inGameMenu
                : UnityEngine.Object.FindFirstObjectByType<UIInGameMenu>(UnityEngine.FindObjectsInactive.Include);
            menu?.HideInventoryPanel();
        }
    }
}
