/* using UnityEngine;

public class Bring : QuestPart
{
    [Header("Bring Settings")]
    public int targetItemID;
    public int requiredAmount = 1;

    [Tooltip("The NPC this quest part must be delivered to")]
    public InteractableNPC targetNPC;

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

        // Track item changes so the description updates
        UpdateProgress();

        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemAdded += HandleItemChanged;
            Inventory.Instance.OnItemRemoved += HandleItemChanged;
        }

        // Subscribe to NPC interactions
        NPCInteractionSystem.Instance.OnNPCInteracted += HandleNPCInteraction;

        Debug.Log($"Bring quest started: Deliver {requiredAmount}x item {targetItemID} to {targetNPC?.name}");
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

    private void HandleNPCInteraction(InteractableNPC npc)
    {
        if (!isActive || isCompleted || isFailed) return;
        if (npc == null || npc != targetNPC) return;

        // Only complete if the player has enough items
        currentAmount = Inventory.Instance.GetItemCount(targetItemID);
        if (currentAmount >= requiredAmount)
        {
            // Remove items from inventory
            Inventory.Instance.RemoveItem(targetItemID, requiredAmount);

            Complete();
        }
        else
        {
            Debug.Log($"Tried to deliver {targetItemID} but only had {currentAmount}/{requiredAmount}.");
        }
    }

    private void UpdateProgress()
    {
        if (Inventory.Instance == null) return;

        currentAmount = Inventory.Instance.GetItemCount(targetItemID);
        parentQuest?.CheckQuestProgress();

        Debug.Log($"Collected {currentAmount}/{requiredAmount} of Item {targetItemID}");
    }

    protected override void OnCompleted()
    {
        base.OnCompleted();
        Unsubscribe();
        Debug.Log($"Bring quest completed: Delivered {requiredAmount}x item {targetItemID} to {targetNPC?.name}");
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

        if (NPCInteractionSystem.Instance != null)
        {
            NPCInteractionSystem.Instance.OnNPCInteracted -= HandleNPCInteraction;
        }
    }
}
*/