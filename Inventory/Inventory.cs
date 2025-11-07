using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ExposedInventory
{
    public int itemID;
    public int amount;
}

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    public Transform inventorySlotsParent;
    public Transform hotbarSlotsParent;
    public Transform equipmentSlotsParent;

    public GameObject SpiritAnimalSlot;
    public GameObject TotemSlot;
    public GameObject ArmorSlot;

    public List<InventorySlot> inventorySlots = new();
    public List<InventorySlot> hotbarSlots = new();
    public List<InventorySlot> equipmentSlots = new();

    public event System.Action<int, int> OnItemAdded;   // (itemID, amount)
    public event System.Action<int, int> OnItemRemoved; // (itemID, amount)

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Prevent duplicates
    }

    private void Start()
    {
        InitializeSlotData();
    }

    void InitializeSlotData()
    {
        inventorySlots.Clear();
        hotbarSlots.Clear();
        equipmentSlots.Clear();

        for (int i = 0; i < inventorySlotsParent.childCount; i++)
        {
            var slot = inventorySlotsParent.GetChild(i).GetComponent<InventorySlot>();
            if (slot != null)
            {
                slot.isHotbarSlot = false;
                slot.slotIndex = i;
                inventorySlots.Add(slot);
                slot.UpdateSlotData();
            }
        }

        for (int i = 0; i < hotbarSlotsParent.childCount; i++)
        {
            var slot = hotbarSlotsParent.GetChild(i).GetComponent<InventorySlot>();
            if (slot != null)
            {
                slot.isHotbarSlot = true;
                slot.slotIndex = i;
                hotbarSlots.Add(slot);
                slot.UpdateSlotData();
            }
        }

        for (int i = 0; i < equipmentSlotsParent.childCount; i++)
        {
            var slot = equipmentSlotsParent.GetChild(i).GetComponent<InventorySlot>();
            if (slot != null)
            {
                slot.isEquipmentSlot = true;
                slot.slotIndex = i;
                equipmentSlots.Add(slot);
                slot.UpdateSlotData();
            }
        }
    }

    public bool AddItem(int itemID, int amount)
    {
        GameObject prefab = ItemDatabase.Instance.Get2DPrefabByID(itemID);
        InventoryItemInfo itemInfo = prefab.GetComponent<InventoryItemInfo>();
        UISystem.Instance.ShowItemAddedPopup(itemID, amount);

        int originalAmount = amount; // remember what we tried to add
        int addedCount = 0;

        foreach (var slot in inventorySlots)
        {
            if (slot.itemID == itemID && slot.itemCount < itemInfo.MaxStackAmount)
            {
                int space = itemInfo.MaxStackAmount - slot.itemCount;
                int toAdd = Mathf.Min(space, amount);

                for (int i = 0; i < toAdd; i++)
                {
                    GameObject item = Instantiate(prefab, slot.transform);
                    AssignDragAndDropReferences(item, itemID);
                }

                slot.UpdateSlotData();
                amount -= toAdd;
                addedCount += toAdd;
                if (amount <= 0)
                {
                    OnItemAdded?.Invoke(itemID, addedCount);
                    return true;
                }
            }
        }

        foreach (var slot in hotbarSlots)
        {
            if (slot.itemID == itemID && slot.itemCount < itemInfo.MaxStackAmount)
            {
                int space = itemInfo.MaxStackAmount - slot.itemCount;
                int toAdd = Mathf.Min(space, amount);

                for (int i = 0; i < toAdd; i++)
                {
                    GameObject item = Instantiate(prefab, slot.transform);
                    AssignDragAndDropReferences(item, itemID);
                }

                slot.UpdateSlotData();
                amount -= toAdd;
                addedCount += toAdd;
                if (amount <= 0)
                {
                    OnItemAdded?.Invoke(itemID, addedCount);
                    return true;
                }
            }
        }

        foreach (var slot in inventorySlots)
        {
            if (slot.itemID == -1)
            {
                int toAdd = Mathf.Min(itemInfo.MaxStackAmount, amount);

                for (int i = 0; i < toAdd; i++)
                {
                    GameObject item = Instantiate(prefab, slot.transform);
                    AssignDragAndDropReferences(item, itemID);
                }

                slot.UpdateSlotData();
                amount -= toAdd;
                addedCount += toAdd;
                if (amount <= 0)
                {
                    OnItemAdded?.Invoke(itemID, addedCount);
                    return true;
                }
            }
        }

        if (addedCount > 0)
            OnItemAdded?.Invoke(itemID, addedCount);

        UISystem.Instance.ShowItemAddedPopup(itemID, amount);
        return false;
    }

    public int GetItemCount(int itemID)
    {
        int total = 0;

        foreach (var slot in inventorySlots)
        {
            if (slot.itemID == itemID)
            {
                total += slot.itemCount;
            }
        }

        foreach (var slot in hotbarSlots)
        {
            if (slot.itemID == itemID)
            {
                total += slot.itemCount;
            }
        }

        // Optional: If you want equipped items to also count
        foreach (var slot in equipmentSlots)
        {
            if (slot.itemID == itemID)
            {
                total += slot.itemCount;
            }
        }

        return total;
    }



    private void AssignDragAndDropReferences(GameObject item, int itemID)
    {
        DragAndDrop drag = item.GetComponent<DragAndDrop>();
        if (drag != null)
        {
            drag.itemID = itemID;
            drag.SpiritAnimalSlot = SpiritAnimalSlot;
            drag.TotemSlot = TotemSlot;
            drag.ArmorSlot = ArmorSlot;
        }
    }

    public bool HasItem(int itemID, int requiredAmount)
    {
        int total = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot.itemID == itemID)
            {
                total += slot.itemCount;
                if (total >= requiredAmount)
                    return true;
            }
        }
        return false;
    }

    public bool RemoveItem(int itemID, int amountToRemove)
    {
        int removedCount = 0;

        foreach (var slot in inventorySlots)
        {
            if (slot.itemID == itemID)
            {
                List<Transform> toDestroy = new List<Transform>();

                foreach (Transform child in slot.transform)
                {
                    if (child.GetComponent<DragAndDrop>().itemID == itemID)
                    {
                        toDestroy.Add(child);
                        removedCount++;
                        amountToRemove--;

                        if (amountToRemove <= 0)
                            break;
                    }
                }

                foreach (Transform item in toDestroy)
                {
                    Destroy(item.gameObject);
                }

                slot.UpdateSlotData();

                if (amountToRemove <= 0)
                {
                    if (removedCount > 0)
                        OnItemRemoved?.Invoke(itemID, removedCount);
                    return true;
                }
            }
        }

        if (removedCount > 0)
            OnItemRemoved?.Invoke(itemID, removedCount);

        return false;
    }

}
