using UnityEngine;

public class Collect : QuestPart
{
    [Header("Collect Settings")]
    public int targetItemID;
    public int requiredAmount = 1;

    private int currentAmount = 0;

    public override string Description
    {
        get
        {
            return base.Description + $" ({currentAmount}/{requiredAmount})";
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        // Check immediately when quest starts
        UpdateProgress();

        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemAdded += HandleItemChanged;
            Inventory.Instance.OnItemRemoved += HandleItemChanged;
        }

        Debug.Log($"Collect quest started: Gather {requiredAmount}x item {targetItemID}");
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void HandleItemChanged(int itemID, int amount)
    {
        if (!isActive || isCompleted || isFailed) return;
        if (itemID == targetItemID) UpdateProgress();
    }

    private void UpdateProgress()
    {
        if (Inventory.Instance == null) return;

        currentAmount = Inventory.Instance.GetItemCount(targetItemID);

        parentQuest?.CheckQuestProgress();

        Debug.Log($"Collected {currentAmount}/{requiredAmount} of Item {targetItemID}");

        if (currentAmount >= requiredAmount)
        {
            Complete();
        }
    }

    protected override void OnCompleted()
    {
        base.OnCompleted();
        Unsubscribe();
        Debug.Log($"Collect quest completed: {requiredAmount}x item {targetItemID} gathered.");
    }

    protected override void OnFailed()
    {
        base.OnFailed();
        Unsubscribe();
    }

    private void Unsubscribe()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemAdded -= HandleItemChanged;
            Inventory.Instance.OnItemRemoved -= HandleItemChanged;
        }
    }
}
