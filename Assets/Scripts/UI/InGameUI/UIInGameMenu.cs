using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class UIInGameMenu : MonoBehaviour
{
    [SerializeField] GameObject inGameMenuPanel;
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] UIInventoryView playerInventoryView;
    [SerializeField] UILootDialog lootDialog;
    [SerializeField] GameObject craftingPanel;
    [SerializeField] GameObject helpPanel;
    private InputSystem_Actions _inputActions;
    private float _previousTimeScale = 1f;
    private Coroutine _bindInventoryRoutine;

    void Awake()
    {
        inGameMenuPanel.SetActive(false);
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        _inputActions ??= new InputSystem_Actions();
        _inputActions.UI.ESC.performed += HandleEsc;
        _inputActions.UI.Inventory.performed += HandleInventory;
        _inputActions.UI.CraftingMenu.performed += HandleCraftingMenu;
        _inputActions.UI.Help.performed += HandleHelp;
        _inputActions.UI.Enable();
        TryStartBindInventory();

        if (lootDialog == null)
        {
            lootDialog = UnityEngine.Object.FindFirstObjectByType<UILootDialog>(UnityEngine.FindObjectsInactive.Include);
        }
    }

    void OnDisable()
    {
        if (_bindInventoryRoutine != null)
        {
            StopCoroutine(_bindInventoryRoutine);
            _bindInventoryRoutine = null;
        }
        if (_inputActions != null)
        {
            _inputActions.UI.ESC.performed -= HandleEsc;
            _inputActions.UI.Inventory.performed -= HandleInventory;
            _inputActions.UI.CraftingMenu.performed -= HandleCraftingMenu;
            _inputActions.UI.Help.performed -= HandleHelp;
            _inputActions.UI.Disable();
        }
    }

    // Show the in-game menu panel (hook up to a button).
    public void ShowMenu()
    {
        if (inGameMenuPanel != null && inGameMenuPanel.activeSelf)
        {
            return;
        }
        _previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        if (inGameMenuPanel != null)
        {
            inGameMenuPanel.SetActive(true);
        }
    }

    // Hide the in-game menu panel (hook up to a button).
    public void HideMenu()
    {
        if (inGameMenuPanel != null && !inGameMenuPanel.activeSelf)
        {
            return;
        }
        Time.timeScale = _previousTimeScale;
        if (inGameMenuPanel != null)
        {
            inGameMenuPanel.SetActive(false);
        }
    }

    // Restart the current scene (hook up to a button).
    public void RestartCurrentScene()
    {
        Time.timeScale = 1f;
        var scene = SceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            SceneManager.LoadScene(scene.buildIndex);
        }
    }

    // Return to the main menu (scene build index 0).
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    // Quit the application (in Editor, stop Play mode).
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void HandleEsc(InputAction.CallbackContext context)
    {
        if (craftingPanel != null && craftingPanel.activeSelf)
        {
            HideCraftingPanel();
            return;
        }

        if (inGameMenuPanel != null && inGameMenuPanel.activeSelf)
        {
            HideMenu();
        }
        else
        {
            ShowMenu();
        }
    }

    void HandleInventory(InputAction.CallbackContext context)
    {
        if (inventoryPanel == null)
        {
            return;
        }

        bool nextActive = !inventoryPanel.activeSelf;
        if (nextActive)
        {
            ShowInventoryPanel();
        }
        else
        {
            inventoryPanel.SetActive(false);
            lootDialog?.Hide();
        }
    }

    void HandleCraftingMenu(InputAction.CallbackContext context)
    {
        ShowCraftingPanel();
    }

    void HandleHelp(InputAction.CallbackContext context)
    {
        if (helpPanel == null)
        {
            return;
        }

        if (helpPanel.activeSelf)
        {
            HideHelpPanel();
        }
        else
        {
            ShowHelpPanel();
        }
    }

    private void TryStartBindInventory()
    {
        if (playerInventoryView == null)
        {
            return;
        }

        if (_bindInventoryRoutine != null)
        {
            return;
        }

        _bindInventoryRoutine = StartCoroutine(BindInventoryWhenReady());
    }

    private System.Collections.IEnumerator BindInventoryWhenReady()
    {
        while (LevelManager.Instance == null || LevelManager.Instance.Player == null)
        {
            yield return null;
        }

        var playerInventory = LevelManager.Instance.Player.Inventory;
        if (playerInventory != null)
        {
            playerInventoryView.Bind(playerInventory.Model);
        }

        _bindInventoryRoutine = null;
    }

    public void ShowInventoryPanel()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }
    }

    public void HideInventoryPanel()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    public void ShowCraftingPanel()
    {
        if (craftingPanel != null)
        {
            craftingPanel.SetActive(true);
        }
    }

    public void HideCraftingPanel()
    {
        if (craftingPanel != null)
        {
            craftingPanel.SetActive(false);
        }
    }

    public void ShowHelpPanel()
    {
        if (helpPanel != null)
        {
            helpPanel.SetActive(true);
        }
    }

    public void HideHelpPanel()
    {
        if (helpPanel != null)
        {
            helpPanel.SetActive(false);
        }
    }
}
