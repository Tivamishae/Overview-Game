using UnityEngine;
using TMPro;

public class Conversation : HumanInteraction
{
    [TextArea]
    public string[] dialogueLines;

    public TextMeshProUGUI dialogueTextUI;

    private int currentLine = 0;
    public bool hasBeenConversed = false;

    public string QuestName;

    private GameObject questPointerInstance;

    void Start()
    {
        // Auto-assign dialogueTextUI if not set in Inspector
        if (dialogueTextUI == null)
        {
            GameObject textObject = GameObject.Find("UI/Popups/NPCQuoteDisplay/Text");
            if (textObject != null)
            {
                dialogueTextUI = textObject.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogWarning("Conversation: Could not find dialogue Text UI at 'UI/Popups/NPCQuoteDisplay/Text'");
            }
        }

        // Spawn quest pointer if conversation hasn't happened yet
        if (!hasBeenConversed)
        {
            GameObject questPointerPrefab = Resources.Load<GameObject>("2D/RuntimeCanvases/NPCQuestPointer");
            if (questPointerPrefab != null)
            {
                questPointerInstance = Instantiate(questPointerPrefab, transform);
                questPointerInstance.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            }
            else
            {
                Debug.LogWarning("Conversation: Could not find NPCQuestPointer prefab in Resources/2D/RuntimeCanvases/");
            }
        }

    }

    public override void Interact()
    {
        currentLine = -1;
        ShowLine();
    }

    public override void Progress()
    {
        currentLine++;
        if (currentLine < dialogueLines.Length)
        {
            var talkClip = Resources.Load<AudioClip>("Sounds/NPC/Talk");
            AudioSystem.Instance.PlayClipFollow(talkClip, transform, 1f);
            ShowLine();
        }
        else
        {
            // End of dialogue
            if (!hasBeenConversed)
            {
                hasBeenConversed = true;
                Debug.Log($"Conversation finished for the first time with NPC '{name}'.");

                QuestSystem.Instance.AddQuest("ReachTheVillage");

                // Remove quest pointer now that conversation is done
                if (questPointerInstance != null)
                {
                    Destroy(questPointerInstance);
                }
            }

            NPCInteractionSystem.Instance.EndInteraction();
        }
    }

    private void ShowLine()
    {
        if (dialogueLines == null || dialogueLines.Length == 0 || dialogueTextUI == null)
            return;

        // Prevent index error when currentLine == -1 on first interact
        if (currentLine >= 0 && currentLine < dialogueLines.Length)
        {
            dialogueTextUI.text = dialogueLines[currentLine];
        }
    }
}
