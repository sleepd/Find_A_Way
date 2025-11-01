using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIMainMenu : MonoBehaviour
{
    [SerializeField] GameObject _mainMenuPanel;
    [SerializeField] GameObject _loadGamePanel;
    [SerializeField] GameObject _optionsPanel;
    [SerializeField] UIDialog _dialogPanelPrefab;
    [SerializeField] Button _quitButton;

    void Awake()
    {
        _mainMenuPanel.SetActive(true);
        _loadGamePanel.SetActive(false);
        _optionsPanel.SetActive(false);
    }

    private void TriggerPanel(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
    }
    public void TriggerLoadGamePanel()
    {
        TriggerPanel(_mainMenuPanel);
        TriggerPanel(_loadGamePanel);
    }

    public void TriggerOptionPanel()
    {
        TriggerPanel(_mainMenuPanel);
        TriggerPanel(_optionsPanel);
    }

    public void OnQuitSelected()
    {
        UIDialog dialog = Instantiate(_dialogPanelPrefab, this.transform);
        dialog.Initialize("Quit", "Are you sure you want to quit the game?", Quit, OnCanceledQuit);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            EditorApplication.ExitPlaymode();
            return;
        }
#endif
        Application.Quit();
    }

    void OnCanceledQuit()
    {
        _quitButton.Select();
    }
}
