using UnityEngine;
using TMPro;
using System.Collections;

public class TimeTransitionUI : MonoBehaviour
{
    public static TimeTransitionUI Instance { get; private set; }

    [Header("UI Settings")]
    public TextMeshProUGUI transitionText;
    public float fadeDuration = 1.5f;
    public float visibleDuration = 2.5f;

    private CanvasGroup canvasGroup;
    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    public void ShowMessage(string message)
    {
        if (transitionText == null) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FadeRoutine(message));
    }

    private IEnumerator FadeRoutine(string message)
    {
        transitionText.text = message;

        // Fade in
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        // Stay visible
        yield return new WaitForSeconds(visibleDuration);

        // Fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}
