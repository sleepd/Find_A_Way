using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] float activeRane = 5f;
    [SerializeField] float fadeDuration = 0.5f;
    CanvasGroup canvasGroup;
    bool isShowing = false;
    Coroutine fadeRoutine;
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }


    void Update()
    {
        if(Vector3.Distance(playerController.transform.position, transform.position) < activeRane)
        {
            if (!isShowing) FadeIn();
            FacePlayerOnY();
        }
        else
        {
            if (isShowing) FadeOut();
        }
    }

    void FacePlayerOnY()
    {
        Vector3 direction = playerController.transform.position - transform.position;
        direction.y = 0f;
        if(direction.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(direction * -1f);
        }
        // transform.LookAt(playerController.transform.forward);
    }

    void FadeIn()
    {
        isShowing = true;
        StartFade(1f);
    }

    void FadeOut()
    {
        isShowing = false;
        StartFade(0f);
    }

    void StartFade(float targetAlpha)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(Fade(targetAlpha));
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = fadeDuration > 0f ? elapsedTime / fadeDuration : 1f;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        fadeRoutine = null;
    }
}
