/* using UnityEngine;
using System.Collections;

public class TalkToNPC : QuestPart
{
    [SerializeField] private InteractableNPC targetNPC;
    [TextArea] public string[] questDialogueLines;

    private Conversation questConversation;

    // Marker
    private GameObject markerPrefab;
    private GameObject spawnedMarker;
    public GameObject SpawnedMarker => spawnedMarker;

    protected override void OnActivated()
    {
        base.OnActivated();

        if (targetNPC == null) return;

        // Load marker prefab
        markerPrefab = Resources.Load<GameObject>("2D/RuntimeCanvases/QuestWaypointWorldspace");

        // Only spawn marker if this quest is the current quest
        if (parentQuest == QuestSystem.Instance.currentQuest && markerPrefab != null)
        {
            SpawnMarker();
        }

        // Create quest-only conversation
        questConversation = targetNPC.gameObject.AddComponent<Conversation>();
        questConversation.dialogueLines = questDialogueLines;
        questConversation.QuestName = parentQuest.name;

        StartCoroutine(WaitForQuestConversation());
    }

    private void Update()
    {
        if (!isActive || isCompleted || isFailed) return;

        //  Remove marker if quest is not current
        if (parentQuest != QuestSystem.Instance.currentQuest)
        {
            RemoveMarker();
            return;
        }

        //  Respawn marker if it was missing and we�re the current quest
        if (spawnedMarker == null && markerPrefab != null)
        {
            SpawnMarker();
        }
    }

    private IEnumerator WaitForQuestConversation()
    {
        while (questConversation != null && !questConversation.hasBeenConversed)
            yield return null;

        Complete();

        if (questConversation != null)
            Destroy(questConversation);

        RemoveMarker();
    }

    private void SpawnMarker()
    {
        if (markerPrefab != null && targetNPC != null)
        {
            RemoveMarker();
            spawnedMarker = Instantiate(markerPrefab, targetNPC.transform);
            spawnedMarker.transform.localPosition = Vector3.up * 2f;
        }
    }

    private void RemoveMarker()
    {
        if (spawnedMarker != null)
        {
            Destroy(spawnedMarker);
            spawnedMarker = null;
        }
    }

    protected override void OnFailed()
    {
        base.OnFailed();
        RemoveMarker();
    }
}
*/