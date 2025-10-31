using UnityEngine;

public class UIMainMenu : MonoBehaviour
{
    [SerializeField] GameObject _mainMenuPanel;
    [SerializeField] GameObject _loadGamePanel;
    [SerializeField] GameObject _optionsPanel;

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
}
