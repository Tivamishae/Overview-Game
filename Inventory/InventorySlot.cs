using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public int itemID = -1;
    public int itemCount = 0;
    public TextMeshProUGUI amountText;
    public bool isHotbarSlot = false;
    public bool isEquipmentSlot = false;
    public int slotIndex = -1;

    [Header("Visual")]
    public Image highlightImage; // Image used for visual feedback
    public Color normalColor = Color.white;
    public Color highlightedColor = new Color(1f, 1f, 1f, 0.75f); // Slightly dimmer or tinted

    private Inventory inventory;

    private void Start()
    {
        inventory = GetComponentInParent<Inventory>();
        UpdateSlotData();
        SetHighlighted(false); // Make sure it's off by default
    }

    private void Update()
    {
        UpdateSlotData();
    }

    public void UpdateSlotData()
    {
        itemCount = 0;
        itemID = -1;

        foreach (Transform child in transform)
        {
            DragAndDrop drag = child.GetComponent<DragAndDrop>();
            if (drag != null)
            {
                if (itemID == -1)
                    itemID = drag.itemID;

                if (drag.itemID == itemID)
                {
                    itemCount++;
                    child.gameObject.SetActive(itemCount == 1);
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        amountText.text = itemCount > 0 ? itemCount.ToString() : "";
        if (itemCount <= 0)
        {
            itemCount = -1;
        }
    }

    public void SetHighlighted(bool active)
    {
        if (highlightImage != null)
            highlightImage.color = active ? highlightedColor : normalColor;
    }

    public bool CanAcceptItem(int incomingID, int incomingAmount, out int acceptedAmount)
    {
        acceptedAmount = 0;

        if (itemID == -1 || itemID == incomingID)
        {
            int space = ItemDatabase.Instance.Get2DPrefabByID(incomingID)
                         .GetComponent<InventoryItemInfo>().MaxStackAmount - itemCount;

            acceptedAmount = Mathf.Min(space, incomingAmount);
            return acceptedAmount > 0;
        }

        return false;
    }
}
