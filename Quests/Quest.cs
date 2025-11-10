using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Quest : MonoBehaviour
{
    [Header("Meta")]
    public string QuestType;
    public string QuestName;

    [TextArea]
    [SerializeField] private string baseDescription;

    public int RewardXP;
    public int RewardMoney;
    public string Type;

    [Header("Visuals")]
    public Sprite QuestIcon;

    [Header("Parts (auto from children or set manually)")]
    [SerializeField] private List<GameObject> questPartObjects = new(); // <-- GameObjects now

    public bool isQuestTrigger = false;
    public string nextQuestName;

    [Header("State")]
    public bool Completed;
    public bool Failed;
    public bool isStarted;
    public bool isCurrentQuest; // Set by QuestSystem

    public event Action<Quest> OnQuestCompleted;
    public event Action<Quest> OnQuestFailed;
    public event Action<Quest> OnQuestPartCompleted;

    // Helper property to get QuestParts from objects
    public IReadOnlyList<QuestPart> Parts
    {
        get
        {
            tempParts.Clear();
            foreach (var go in questPartObjects)
            {
                if (go != null)
                {
                    var part = go.GetComponent<QuestPart>();
                    if (part != null)
                        tempParts.Add(part);
                }
            }
            return tempParts;
        }
    }
    private readonly List<QuestPart> tempParts = new();

    public string Description
    {
        get
        {
            foreach (var part in Parts)
                if (part != null && part.isActive)
                    return part.Description;
            return baseDescription;
        }
    }

    private void Awake()
    {
        if (questPartObjects == null || questPartObjects.Count == 0)
            RefreshPartsFromChildren();

        EnsureParentLinks();
        // Do NOT activate here � QuestSystem will start it
    }

    private void OnValidate()
    {
        if (questPartObjects == null) questPartObjects = new List<GameObject>();
        EnsureParentLinks();
    }

    [ContextMenu("Refresh Parts From Children")]
    public void RefreshPartsFromChildren()
    {
        questPartObjects ??= new List<GameObject>();
        questPartObjects.Clear();

        var parts = GetComponentsInChildren<QuestPart>(true);
        Array.Sort(parts, (a, b) => GetHierarchyOrder(a.transform).CompareTo(GetHierarchyOrder(b.transform)));

        foreach (var p in parts)
            if (p != null) questPartObjects.Add(p.gameObject);

        EnsureParentLinks();
    }

    private static int GetHierarchyOrder(Transform t)
    {
        int order = 0;
        int mul = 1;
        while (t != null)
        {
            order += (t.GetSiblingIndex() + 1) * mul;
            mul *= 1000;
            t = t.parent;
        }
        return order;
    }

    private void EnsureParentLinks()
    {
        foreach (var go in questPartObjects)
        {
            if (go != null)
            {
                var part = go.GetComponent<QuestPart>();
                if (part != null)
                    part.parentQuest = this;
            }
        }
    }

    private void ActivateFirstIfNeeded()
    {
        if (!isStarted) return;
        if (Parts.Count == 0) return;

        bool anyStateSet = false;
        foreach (var part in Parts)
        {
            if (part == null) continue;
            if (part.isActive || part.isCompleted || part.isFailed)
            {
                anyStateSet = true;
                break;
            }
        }

        if (!anyStateSet && Parts[0] != null)
            Parts[0].SetActive(true);
    }

    public void StartQuest()
    {
        isStarted = true;
        ActivateFirstIfNeeded();
    }

    public void CheckQuestProgress()
    {
        if (Parts.Count == 0) return;
        EnsureParentLinks();

        for (int i = 0; i < Parts.Count; i++)
        {
            QuestPart part = Parts[i];
            if (part == null) continue;

            if (part.isFailed)
            {
                Failed = true;
                Completed = false;
                OnQuestFailed?.Invoke(this);
                return;
            }

            if (part.isActive)
                return;

            if (part.isCompleted)
            {
                if (i + 1 < Parts.Count)
                {
                    QuestPart nextPart = Parts[i + 1];
                    if (nextPart != null && !nextPart.isActive && !nextPart.isCompleted && !nextPart.isFailed)
                    {
                        nextPart.SetActive(true);
                        OnQuestPartCompleted?.Invoke(this);
                        return;
                    }
                }
                continue;
            }

            if (!part.isCompleted)
                return;
        }

        Completed = true;
        Failed = false;
        OnQuestCompleted?.Invoke(this);
    }

    public void SetIsCurrentQuest(bool value)
    {
        isCurrentQuest = value;
    }
}
