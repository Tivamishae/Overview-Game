using UnityEngine;
using System.Collections.Generic;

public class HotbarLogic : MonoBehaviour
{
    public static HotbarLogic Instance { get; private set; }
    public Transform equipmentHolder;
    public GameObject currentEquipped;
    private int currentSlotIndex = -1;
    public int itemEquippedInHotbarID = -1;
    public ArmMovements armAnimator;

    public List<InventorySlot> hotbarSlotScripts = new List<InventorySlot>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        hotbarSlotScripts = Inventory.Instance.hotbarSlots;
    }

    private void Update()
    {
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                HandleHotbarKeyPress(i);
            }
        }
    }

    private void HandleHotbarKeyPress(int index)
    {
        if (currentSlotIndex == index)
        {
            UnequipItem();
            return;
        }

        EquipItem(index);
    }

    public void UpdateEquippedItemInHotbar(int index)
    {
        if (index >= 0 && index < hotbarSlotScripts.Count)
        {
            itemEquippedInHotbarID = hotbarSlotScripts[index].itemID;
        }
        else
        {
            itemEquippedInHotbarID = -1;
        }
    }

    public void EquipItem(int index)
    {
        armAnimator.EquipItem(true);
        if (index < 0 || index >= hotbarSlotScripts.Count)
        {
            Debug.LogWarning($"Hotbar index {index} is out of range.");
            return;
        }

        InventorySlot slot = hotbarSlotScripts[index];

        if (slot == null || slot.itemID == -1 || slot.itemCount <= 0)
        {
            Debug.Log($"Hotbar slot {index + 1} is empty.");
            return;
        }

        GameObject prefab = ItemDatabase.Instance.GetEquipmentPrefabByID(slot.itemID);

        if (prefab == null)
        {
            Debug.LogWarning($"No equipment prefab found for item ID {slot.itemID}");
            return;
        }

        if (currentEquipped != null)
        {
            Destroy(currentEquipped);
        }

        UpdateEquippedItemInHotbar(index);

        currentEquipped = Instantiate(prefab, equipmentHolder);

        EquipmentItem equipmentItem = currentEquipped.GetComponent<EquipmentItem>();
        if (equipmentItem != null)
        {
            currentEquipped.transform.localPosition = equipmentItem.localPositionOffset;
            currentEquipped.transform.localEulerAngles = equipmentItem.localRotationOffset;
        }
        else
        {
            currentEquipped.transform.localPosition = Vector3.zero;
            currentEquipped.transform.localRotation = Quaternion.identity;
        }

        HighlightSlot(index);
        currentSlotIndex = index;
    }

    public void UnequipItem()
    {
        armAnimator.EquipItem(false);
        if (currentEquipped != null)
        {
            Destroy(currentEquipped);
            currentEquipped = null;
        }

        currentSlotIndex = -1;
        itemEquippedInHotbarID = -1; //  Reset equipped item ID
        HighlightSlot(-1);
        Debug.Log("Item unequipped.");
    }


    private void HighlightSlot(int index)
    {
        for (int i = 0; i < hotbarSlotScripts.Count; i++)
        {
            bool isActive = (i == index);
            hotbarSlotScripts[i].SetHighlighted(isActive);
        }
    }
}
