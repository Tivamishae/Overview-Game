using UnityEngine;

public abstract class QuestPart : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string description;

    [HideInInspector] public Quest parentQuest;

    [Header("State")]
    public bool isActive;
    public bool isCompleted;
    public bool isFailed;

    public virtual string Description => description;

    protected virtual void OnEnable()
    {
        if (parentQuest == null)
            parentQuest = GetComponentInParent<Quest>();
    }

    public virtual void SetActive(bool value)
    {
        // Only allow activation if the parent quest is started
        if (value && (parentQuest == null || !parentQuest.isStarted))
            return;

        isActive = value;
        if (isActive) OnActivated();
    }


    /// <summary>Called when the part becomes active.</summary>
    protected virtual void OnActivated() { }

    public virtual void Complete()
    {
        if (isCompleted || isFailed) return;
        isActive = false;
        isCompleted = true;
        OnCompleted();
        parentQuest?.CheckQuestProgress();
    }

    protected virtual void OnCompleted() { }

    public virtual void Fail()
    {
        if (isCompleted || isFailed) return;
        isActive = false;
        isFailed = true;
        OnFailed();
        parentQuest?.CheckQuestProgress();
    }

    protected virtual void OnFailed() { }
}
