using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq; // For LINQ's FirstOrDefault

public class QuestSystem : MonoBehaviour
{
    public static QuestSystem Instance { get; private set; }

    [Header("UI References")]
    public GameObject questScreen;
    public Transform questsContent;

    [Header("No Quest Displays")]
    public GameObject NoQuestsDisplay;
    public GameObject NoCurrentQuestDisplay;

    [Header("UI Buttons")]
    public Button questsButton;
    public Button currentQuestButton;
    public Button completedQuestsButton;
    public Button failedQuestsButton;

    [Header("Quest Lists")]
    public List<Quest> quests = new();
    public List<Quest> completedQuests = new();
    public List<Quest> failedQuests = new();

    [Header("Current Quest")]
    public Quest currentQuest;

    public GameObject CurrentQuestPopup;     // Parent popup object (starts inactive)
    public TMP_Text PopupQuestName;          // Child: "QuestName"
    public TMP_Text PopupQuestDescription;   // Child: "QuestDescription"
    public Image PopupQuestIcon;             // Child: "QuestIcon"

    private GameObject questCellPrefab;

    [Header("Animations")]
    public QuestPopupAnimator completedDisplay;
    public QuestPopupAnimator newQuestDisplay;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        questCellPrefab = Resources.Load<GameObject>("2D/InventoryDisplays/QuestCell");
        if (questCellPrefab == null)
            Debug.LogError("QuestCell prefab not found in Resources/2D/InventoryDisplays/");

        // Hook up buttons to their functions
        if (questsButton != null)
            questsButton.onClick.AddListener(ShowQuests);

        if (currentQuestButton != null)
            currentQuestButton.onClick.AddListener(ShowCurrentQuest);

        if (completedQuestsButton != null)
            completedQuestsButton.onClick.AddListener(ShowCompletedQuests);

        if (failedQuestsButton != null)
            failedQuestsButton.onClick.AddListener(ShowFailedQuests);
    }

    private void Start()
    {
        // Ensure both displays are hidden at start
        if (NoQuestsDisplay != null)
            NoQuestsDisplay.SetActive(false);

        if (NoCurrentQuestDisplay != null)
            NoCurrentQuestDisplay.SetActive(false);
    }

    public void SetCurrentQuest(Quest q)
    {
        if (quests.Contains(q))
        {
            currentQuest = q;

            // Update all quest flags
            foreach (var quest in quests)
                quest.SetIsCurrentQuest(quest == currentQuest);

            Debug.Log("Current quest set to: " + q.QuestName);
        }
    }


    public void RefreshQuestUI(List<Quest> listToShow, bool allowSelection)
    {
        if (questsContent == null || questCellPrefab == null) return;

        // Clear content
        foreach (Transform child in questsContent)
            Destroy(child.gameObject);

        // Hide both "No quests" messages by default
        if (NoQuestsDisplay != null)
            NoQuestsDisplay.SetActive(false);

        if (NoCurrentQuestDisplay != null)
            NoCurrentQuestDisplay.SetActive(false);

        // Show "NoQuestsDisplay" if list is empty
        if (listToShow == null || listToShow.Count == 0)
        {
            if (NoQuestsDisplay != null)
                NoQuestsDisplay.SetActive(true);
            return; // nothing to show
        }

        // Populate list
        foreach (Quest quest in listToShow)
        {
            GameObject cell = Instantiate(questCellPrefab, questsContent);

            QuestCellUI cellUI = cell.GetComponent<QuestCellUI>();
            if (cellUI != null)
            {
                cellUI.SetQuest(quest, allowSelection);
            }
        }
    }

    public void RefreshCurrentQuestPopup()
    {
        if (!CurrentQuestPopup) return;

        // Popup is visible only when quest screen is CLOSED and we have a current quest
        bool questScreenOpen = questScreen && questScreen.activeInHierarchy;

        if (PopupQuestName) PopupQuestName.text = currentQuest.QuestName ?? "";
        if (PopupQuestDescription) PopupQuestDescription.text = currentQuest.Description ?? "";
        if (PopupQuestIcon) PopupQuestIcon.sprite = currentQuest.QuestIcon;
    }


    // --- UI Button Functions ---
    public void ShowQuests()
    {
        RefreshQuestUI(quests, true); // selectable
    }

    public void ShowCurrentQuest()
    {
        // Hide the no-quest UI for normal quest lists

        if (currentQuest != null)
        {
            RefreshQuestUI(new List<Quest> { currentQuest }, true);

            if (NoCurrentQuestDisplay != null)
                NoCurrentQuestDisplay.SetActive(false);
        }
        else
        {
            RefreshQuestUI(new List<Quest>(), true); // will show empty
                                                     // No current quest
            if (NoQuestsDisplay != null)
                NoQuestsDisplay.SetActive(false);

            if (NoCurrentQuestDisplay != null)
                NoCurrentQuestDisplay.SetActive(true);
        }
    }

    public void ShowCompletedQuests()
    {
        RefreshQuestUI(completedQuests, false); // not selectable
    }

    public void ShowFailedQuests()
    {
        RefreshQuestUI(failedQuests, false); // not selectable
    }

    public void AddQuest(string questName)
    {
        newQuestDisplay.ShowNewQuest();
        // Find the "Quests" folder object in the scene
        var questsFolder = UnityEngine.SceneManagement.SceneManager
            .GetActiveScene()
            .GetRootGameObjects()
            .FirstOrDefault(go => go.name.Equals("Quests", System.StringComparison.OrdinalIgnoreCase));

        if (questsFolder == null)
        {
            Debug.LogError("No GameObject named 'Quests' found in the scene hierarchy.");
            return;
        }

        // Find the child GameObject with the matching name
        Transform questTransform = questsFolder.transform.Find(questName);
        if (questTransform == null)
        {
            Debug.LogError($"Quest '{questName}' not found as a child of 'Quests' in the hierarchy.");
            return;
        }

        // Get the Quest component from that child
        Quest foundQuest = questTransform.GetComponent<Quest>();
        if (foundQuest == null)
        {
            Debug.LogError($"GameObject '{questName}' exists under 'Quests' but has no Quest component.");
            return;
        }

        // Add to active quest list if not already there
        if (!quests.Contains(foundQuest))
        {
            quests.Add(foundQuest);

            // Subscribe to completion/fail events
            foundQuest.OnQuestCompleted += HandleQuestCompleted;
            foundQuest.OnQuestFailed += HandleQuestFailed;
            foundQuest.OnQuestPartCompleted += HandleQuestPartCompleted;

            foundQuest.StartQuest(); // This sets isStarted and activates the first part
            Debug.Log($"Quest '{questName}' added to quests list and started.");
        }
        else
        {
            Debug.LogWarning($"Quest '{questName}' is already in the quests list.");
        }
    }

    private void HandleQuestCompleted(Quest quest)
    {
        if (quests.Contains(quest))
            quests.Remove(quest);

        if (!completedQuests.Contains(quest))
            completedQuests.Add(quest);

        bool wasCurrent = (currentQuest == quest);

        if (wasCurrent)
            currentQuest = null;

        quest.isStarted = false;
        quest.SetIsCurrentQuest(false);
        PlayerStats.Instance.Money += quest.RewardMoney;
        PlayerStats.Instance.XP += quest.RewardXP;

        Debug.Log($"Quest '{quest.QuestName}' completed and moved to Completed Quests list.");

        //  Show "Quest Completed" banner
        if (completedDisplay != null)
        {
            completedDisplay.Show(quest.QuestName);
        }

        if (quest.isQuestTrigger)
        {
            AddQuest(quest.nextQuestName);
        }
    }

    private void HandleQuestPartCompleted(Quest quest)
    {
        if (completedDisplay != null)
        {
            completedDisplay.ShowQuestPartCompleted();
        }
    }


    private void HandleQuestFailed(Quest quest)
    {
        if (quests.Contains(quest))
            quests.Remove(quest);

        if (!failedQuests.Contains(quest))
            failedQuests.Add(quest);

        if (currentQuest == quest)
            currentQuest = null;

        quest.isStarted = false;
        quest.SetIsCurrentQuest(false);

        Debug.Log($"Quest '{quest.QuestName}' failed and moved to Failed Quests list.");
    }

    public void ClearCurrentQuest()
    {
        if (currentQuest != null)
        {
            currentQuest.SetIsCurrentQuest(false);
            Debug.Log($"Current quest '{currentQuest.QuestName}' cleared.");
            currentQuest = null;
            RefreshCurrentQuestPopup();

            // Refresh the Current Quest screen UI as well
            ShowCurrentQuest();
        }
    }





}
