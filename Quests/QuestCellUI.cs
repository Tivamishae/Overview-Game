using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestCellUI : MonoBehaviour
{
    [Header("UI References")]
    public Image questIcon;
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI questRewardText;

    public Quest linkedQuest;
    private bool canSelect;

    private void Awake()
    {
        // Auto-assign questIcon from "Icon" child if not assigned in Inspector
        if (questIcon == null)
        {
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null)
                questIcon = iconTransform.GetComponent<Image>();
        }

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning("QuestCellUI is missing a Button component!");
        }
    }

    /// <summary>
    /// Fills the UI fields from a Quest, sets selection ability, and applies optional tint.
    /// </summary>
    public void SetQuest(Quest quest, bool allowSelection)
    {
        linkedQuest = quest;
        canSelect = allowSelection;

        if (questIcon != null && quest.QuestIcon != null)
            questIcon.sprite = quest.QuestIcon;

        if (questNameText != null)
            questNameText.text = quest.QuestName;

        if (questDescriptionText != null)
            questDescriptionText.text = quest.Description;

        if (questRewardText != null)
            questRewardText.text = $"{quest.RewardXP} XP\n{quest.RewardMoney} Money";

        // Tint background if completed or failed
        Image bg = GetComponent<Image>();
        if (bg != null)
        {
            if (quest.Completed)
                bg.color = new Color(0.7f, 1f, 0.7f); // light green
            else if (quest.Failed)
                bg.color = new Color(1f, 0.7f, 0.7f); // light red
            else
                bg.color = Color.white;
        }
    }

    private void OnButtonClicked()
    {
        if (linkedQuest != null && canSelect)
        {
            if (QuestSystem.Instance.currentQuest == linkedQuest)
            {
                // Already the current quest  remove it
                QuestSystem.Instance.ClearCurrentQuest();
            }
            else
            {
                // Not current quest  set it
                QuestSystem.Instance.SetCurrentQuest(linkedQuest);
            }
        }
        else if (!canSelect)
        {
            Debug.Log("This quest cannot be selected.");
        }
    }

}
