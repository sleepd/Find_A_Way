using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScene : MonoBehaviour
{
    // Load the next scene in Build Settings, if one exists.
    public void LoadNextScene()
    {
        var current = SceneManager.GetActiveScene();
        if (!current.IsValid())
        {
            return;
        }

        int nextIndex = current.buildIndex + 1;
        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("[IntroScene] No next scene configured in Build Settings.");
            return;
        }

        SceneManager.LoadScene(nextIndex);
    }
}
