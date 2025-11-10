using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradingCellScript : MonoBehaviour
{
    [Header("Item Info")]
    public int itemID;

    private Button addButton;
    private Button decreaseButton;
    private TextMeshProUGUI amountText;
    private TextMeshProUGUI costText;

    private int amount = 0;

    void Start()
    {
        // Find buttons and text components by name
        addButton = transform.Find("Add")?.GetComponent<Button>();
        decreaseButton = transform.Find("Decrease")?.GetComponent<Button>();
        amountText = transform.Find("Amount")?.GetComponent<TextMeshProUGUI>();
        costText = transform.Find("Price")?.GetComponent<TextMeshProUGUI>();

        if (addButton != null)
            addButton.onClick.AddListener(IncreaseAmount);
        else
            Debug.LogWarning("Add button not found in TradingCell.");

        if (decreaseButton != null)
            decreaseButton.onClick.AddListener(DecreaseAmount);
        else
            Debug.LogWarning("Decrease button not found in TradingCell.");

        if (amountText == null)
            Debug.LogWarning("Amount TextMeshProUGUI not found in TradingCell.");

        if (costText == null)
            Debug.LogWarning("Cost TextMeshProUGUI not found in TradingCell.");

        UpdateAmountDisplay();
        UpdateCostDisplay(); //  safe to do here � all UI refs are initialized now
    }

    private void IncreaseAmount()
    {
        amount++;
        UpdateAmountDisplay();
    }

    private void DecreaseAmount()
    {
        if (amount > 0)
        {
            amount--;
            UpdateAmountDisplay();
        }
    }

    private void UpdateAmountDisplay()
    {
        if (amountText != null)
            amountText.text = amount.ToString();
    }

    private void UpdateCostDisplay()
    {
        if (costText == null) return;

        var prefab = ItemDatabase.Instance.Get2DPrefabByID(itemID);
        if (prefab != null)
        {
            var info = prefab.GetComponent<InventoryItemInfo>();
            if (info != null)
            {
                costText.text = info.Price + "g";
                return;
            }
        }

        costText.text = "??g";
        Debug.LogWarning($"Could not fetch price for itemID {itemID}");
    }

    public int GetCurrentAmount() => amount;

    public int GetItemID() => itemID;

    public void SetItemID(int id)
    {
        itemID = id;
    }
}
