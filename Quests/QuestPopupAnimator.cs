using UnityEngine;
using TMPro;
using System.Collections;

public class QuestPopupAnimator : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text completedText;   // Assign TMP text in Inspector

    [Header("Animation Settings")]

    [Header("Audio")]
    public string questFinishedPath = "Sounds/Quests/QuestFinished"; // Resources path
    public string questPartFinishedPath = "Sounds/Quests/QuestPartFinished";
    private AudioClip questPartFinishedClip;
    private AudioClip questFinishedClip;

    private CanvasGroup canvasGroup;
    private Coroutine routine;

    private void Awake()
    {
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);

        // Load audio once
        questFinishedClip = Resources.Load<AudioClip>(questFinishedPath);
        questPartFinishedClip = Resources.Load<AudioClip>(questPartFinishedPath);

        if (questFinishedClip == null)
            Debug.LogError($"QuestPopupAnimator: Could not find clip at Resources/{questFinishedPath}");
    }

    public void Show(string questName)
    {
        if (routine != null)
            StopCoroutine(routine);

        gameObject.SetActive(true);
        completedText.text = $"Quest Completed:\n{questName}";

        //  Play quest finished sound following the player (or camera if no player found)
        if (questFinishedClip != null && AudioSystem.Instance != null)
        {
            Transform followTarget = null;

            // Try to follow player first
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) followTarget = player.transform;
            else if (Camera.main != null) followTarget = Camera.main.transform;

            if (followTarget != null)
                AudioSystem.Instance.PlayClipFollow(questFinishedClip, followTarget, 1f);
        }

        routine = StartCoroutine(ShowAndFadeOut(4f, 1f));
    }

    public void ShowQuestPartCompleted()
    {
        if (routine != null)
            StopCoroutine(routine);

        gameObject.SetActive(true);
        completedText.text = $"Quest Part Completed";

        //  Play quest finished sound following the player (or camera if no player found)
        if (questPartFinishedClip != null && AudioSystem.Instance != null)
        {
            Transform followTarget = null;

            // Try to follow player first
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) followTarget = player.transform;
            else if (Camera.main != null) followTarget = Camera.main.transform;

            if (followTarget != null)
                AudioSystem.Instance.PlayClipFollow(questPartFinishedClip, followTarget, 1f);
        }

        routine = StartCoroutine(ShowAndFadeOut(1f, 0.5f));
    }

    public void ShowNewQuest()
    {
        if (routine != null)
            StopCoroutine(routine);

        gameObject.SetActive(true);
        completedText.text = "New Quest Started";

        //  Play quest finished sound following the player (or camera if no player found)
        if (questPartFinishedClip != null && AudioSystem.Instance != null)
        {
            Transform followTarget = null;

            // Try to follow player first
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) followTarget = player.transform;
            else if (Camera.main != null) followTarget = Camera.main.transform;

            if (followTarget != null)
                AudioSystem.Instance.PlayClipFollow(questPartFinishedClip, followTarget, 1f);
        }

        routine = StartCoroutine(ShowAndFadeOut(1f, 0.5f));
    }

    private IEnumerator ShowAndFadeOut(float displayTime, float fadeTime)
    {
        // Fade in
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Stay visible
        yield return new WaitForSeconds(displayTime);

        // Fade out
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        routine = null;
    }
}
