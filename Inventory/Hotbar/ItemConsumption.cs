using UnityEngine;
using System.Collections;

public class ItemConsumption : MonoBehaviour
{
    public static ItemConsumption Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ConsumeEquippedItem()
    {
        int equippedItemID = HotbarLogic.Instance.itemEquippedInHotbarID;

        // Apply effects
        ConsumableObject consumable = ItemDatabase.Instance.Get2DPrefabByID(equippedItemID).GetComponent<ConsumableObject>();
        PlayerStats.Instance.Health += consumable.HealthRestoration;
        PlayerStats.Instance.Hunger += consumable.HungerRestoration;
        PlayerStats.Instance.Stamina += consumable.StaminaRestoration;

        // Find and remove 1 stack of the item from the corresponding hotbar slot
        foreach (InventorySlot slot in Inventory.Instance.hotbarSlots)
        {
            if (slot.itemID == equippedItemID)
            {
                // Find and destroy one child (one stack of this item)
                foreach (Transform child in slot.transform)
                {
                    DragAndDrop d = child.GetComponent<DragAndDrop>();
                    if (d != null && d.itemID == equippedItemID)
                    {
                        Destroy(child.gameObject);
                        break;
                    }
                }

                // Update slot data
                slot.UpdateSlotData();

                // Start coroutine to wait a frame before checking and unequipping
                StartCoroutine(DelayedUnequipCheck(slot));
                break;
            }
        }

        Debug.Log($"Consumed item {equippedItemID} and applied stats.");
    }

    private IEnumerator DelayedUnequipCheck(InventorySlot slot)
    {
        yield return null; // wait one frame

        if (slot.itemCount <= 0)
        {
            HotbarLogic.Instance.UnequipItem();
        }
    }
}
