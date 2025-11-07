using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int itemID;
    public bool IsAbleToBeInHotbar;

    [Header("Equipment Slot References")]
    public GameObject SpiritAnimalSlot;
    public GameObject TotemSlot;
    public GameObject ArmorSlot;

    private Transform originalParent;
    private Vector2 originalPosition;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = transform.localPosition;
        transform.SetParent(originalParent.root); // Bring to front
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        GameObject targetObj = eventData.pointerEnter;
        if (targetObj == null)
        {
            ReturnToOriginalSlot();
            return;
        }

        InventorySlot targetSlot = targetObj.GetComponent<InventorySlot>() ?? targetObj.GetComponentInParent<InventorySlot>();
        InventorySlot originSlot = originalParent.GetComponent<InventorySlot>();

        if (targetSlot == null || originSlot == null)
        {
            ReturnToOriginalSlot();
            return;
        }

        // Check if the slot is an equipment slot
        if (targetSlot.isEquipmentSlot)
        {
            if (targetSlot.gameObject == SpiritAnimalSlot)
            {
                ReturnToOriginalSlot(); // Not handled yet
                return;
            }
            else if (targetSlot.gameObject == TotemSlot)
            {
                Totem totem = GetComponent<Totem>();
                if (totem != null)
                {
                    transform.SetParent(targetSlot.transform);
                    transform.localPosition = Vector3.zero;
                    targetSlot.UpdateSlotData();
                    originSlot.UpdateSlotData();
                    return;
                }
                else
                {
                    ReturnToOriginalSlot(); // Not a Totem item
                    return;
                }
            }
            else if (targetSlot.gameObject == ArmorSlot)
            {
                Armor armor = GetComponent<Armor>();
                if (armor != null)
                {
                    transform.SetParent(targetSlot.transform);
                    transform.localPosition = Vector3.zero;
                    targetSlot.UpdateSlotData();
                    originSlot.UpdateSlotData();
                    return;
                }
                else
                {
                    ReturnToOriginalSlot(); // Not an Armor item
                    return;
                }
            }

            // If it's an equipment slot but not one of the known ones, reject
            ReturnToOriginalSlot();
            return;
        }

        // Check hotbar restriction
        if (!IsAbleToBeInHotbar && targetSlot.isHotbarSlot)
        {
            ReturnToOriginalSlot();
            return;
        }

        // Return dragged item first (so we can cleanly collect and reassign)
        ReturnToOriginalSlot();

        // Gather all valid items from the origin slot (same ID)
        var itemsToMove = new List<Transform>();
        foreach (Transform child in originSlot.transform)
        {
            DragAndDrop drag = child.GetComponent<DragAndDrop>();
            if (drag != null && drag.itemID == itemID)
            {
                itemsToMove.Add(child);
            }
        }

        // Total amount we're trying to move
        int totalToMove = itemsToMove.Count;

        // Check if the target slot can accept them
        if (!targetSlot.CanAcceptItem(itemID, totalToMove, out int acceptedAmount))
        {
            return;
        }

        // Move up to acceptedAmount items
        for (int i = 0; i < acceptedAmount && itemsToMove.Count > 0; i++)
        {
            Transform item = itemsToMove[0];
            itemsToMove.RemoveAt(0);

            item.SetParent(targetSlot.transform);
            item.localPosition = Vector3.zero;
        }

        // Update both slots
        originSlot.UpdateSlotData();
        targetSlot.UpdateSlotData();
    }

    private void ReturnToOriginalSlot()
    {
        transform.SetParent(originalParent);
        transform.localPosition = originalPosition;
    }
}
