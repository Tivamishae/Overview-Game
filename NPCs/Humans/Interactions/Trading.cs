using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Trading : HumanInteraction
{
    [Header("Merchant Trade Setup")]
    public int[] itemsForSale;
    public int[] itemsToBuy;

    private List<TradingCellScript> activeCells = new List<TradingCellScript>();

    private GameObject tradingDisplay;
    private Transform contentHolder;
    private GameObject tradingCellPrefab;

    private bool isTrading = false;

    // UI Buttons
    private Button buyButton;
    private Button sellButton;
    private Button confirmButton; // <-- new Trading button

    private enum TradeMode { Buy, Sell }
    private TradeMode currentMode = TradeMode.Buy;

    void Start()
    {
        tradingDisplay = GameObject.Find("UI/Popups/TradingDisplay");

        if (tradingDisplay == null)
        {
            Debug.LogError("TradingDisplay not found in scene!");
        }
        else
        {
            contentHolder = tradingDisplay.transform.Find("ItemHolder/ViewPort/Content");
            if (contentHolder == null)
            {
                Debug.LogError("Content holder not found in TradingDisplay.");
            }

            // Hook up Buy/Sell buttons
            Transform buyTransform = tradingDisplay.transform.Find("Buy");
            Transform sellTransform = tradingDisplay.transform.Find("Sell");
            Transform confirmTransform = tradingDisplay.transform.Find("Trade"); // <-- the "confirm" button

            if (buyTransform != null)
                buyButton = buyTransform.GetComponent<Button>();
            if (sellTransform != null)
                sellButton = sellTransform.GetComponent<Button>();
            if (confirmTransform != null)
                confirmButton = confirmTransform.GetComponent<Button>();

            if (buyButton != null)
                buyButton.onClick.AddListener(() => SwitchMode(TradeMode.Buy));
            if (sellButton != null)
                sellButton.onClick.AddListener(() => SwitchMode(TradeMode.Sell));
            if (confirmButton != null)
                confirmButton.onClick.AddListener(ConfirmTrade);
        }

        tradingCellPrefab = Resources.Load<GameObject>("2D/InventoryDisplays/TradingCell");
        if (tradingCellPrefab == null)
        {
            Debug.LogError("TradingCell prefab not found.");
        }
    }

    void Update()
    {
        if (isTrading && Input.GetKeyDown(KeyCode.E))
        {
            EndTradingInteraction();
        }
    }

    public override void Interact()
    {
        if (tradingDisplay == null || contentHolder == null || tradingCellPrefab == null)
            return;

        tradingDisplay.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Start delayed trading flag
        StartCoroutine(EnableTradingAfterDelay(0.2f));

        // Default to Buy Mode
        currentMode = TradeMode.Buy;
        PopulateTradingItems(itemsForSale);
    }

    private IEnumerator EnableTradingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isTrading = true;
    }


    private void SwitchMode(TradeMode mode)
    {
        if (mode == currentMode) return;

        currentMode = mode;

        if (mode == TradeMode.Buy)
            PopulateTradingItems(itemsForSale);
        else
            PopulateTradingItems(itemsToBuy);
    }

    private void PopulateTradingItems(int[] itemIDs)
    {
        // Clear old content and references
        foreach (Transform child in contentHolder)
        {
            Destroy(child.gameObject);
        }
        activeCells.Clear();

        foreach (int itemID in itemIDs)
        {
            GameObject cell = Instantiate(tradingCellPrefab, contentHolder);
            Sprite sprite = ItemDatabase.Instance.GetItemSpriteByID(itemID);

            if (sprite != null)
            {
                Transform iconTransform = cell.transform.Find("Icon");
                if (iconTransform != null)
                {
                    Image imageComponent = iconTransform.GetComponent<Image>();
                    if (imageComponent != null)
                    {
                        imageComponent.sprite = sprite;
                    }
                }
            }

            TradingCellScript cellScript = cell.GetComponent<TradingCellScript>();
            if (cellScript != null)
            {
                cellScript.SetItemID(itemID);
                activeCells.Add(cellScript);
            }
        }
    }

    public Dictionary<int, int> GetSelectedTradeAmounts()
    {
        Dictionary<int, int> selectedItems = new Dictionary<int, int>();

        foreach (TradingCellScript cell in activeCells)
        {
            int amount = cell.GetCurrentAmount();
            if (amount > 0)
            {
                selectedItems[cell.GetItemID()] = amount;
            }
        }

        return selectedItems;
    }

    public void ConfirmTrade()
    {
        Dictionary<int, int> selection = GetSelectedTradeAmounts();

        if (selection.Count == 0)
        {
            Debug.Log("No items selected for trade.");
            return;
        }

        if (currentMode == TradeMode.Sell)
        {
            // Step 1: Validate all selected items exist in inventory
            foreach (var entry in selection)
            {
                int itemID = entry.Key;
                int amount = entry.Value;

                if (!Inventory.Instance.HasItem(itemID, amount))
                {
                    Debug.LogWarning($"Cannot sell {amount} of item ID {itemID}: not enough in inventory.");
                    return; // cancel full trade
                }
            }
        }
        else if (currentMode == TradeMode.Buy)
        {
            // Step 1: Check if player can afford entire purchase
            int totalCost = 0;
            foreach (var entry in selection)
            {
                int itemID = entry.Key;
                int amount = entry.Value;
                int price = ItemDatabase.Instance.GetItemPriceByID(itemID);
                totalCost += price * amount;
            }

            if (PlayerStats.Instance.Money < totalCost)
            {
                Debug.LogWarning($"Not enough money. Needed: {totalCost}, Available: {PlayerStats.Instance.Money}");
                return;
            }
        }

        // Step 2: Execute the trade
        foreach (var entry in selection)
        {
            int itemID = entry.Key;
            int amount = entry.Value;
            int price = ItemDatabase.Instance.GetItemPriceByID(itemID);
            int total = price * amount;

            if (currentMode == TradeMode.Buy)
            {
                Debug.Log($"Attempting to BUY {amount} of item ID {itemID} for {total}");

                bool success = Inventory.Instance.AddItem(itemID, amount);
                if (success)
                {
                    PlayerStats.Instance.Money -= total;
                    Debug.Log($"Bought {amount} of item ID {itemID}. Money left: {PlayerStats.Instance.Money}");
                }
                else
                {
                    Debug.LogWarning($"Failed to add {amount} of item ID {itemID} to inventory");
                }
            }
            else if (currentMode == TradeMode.Sell)
            {
                Debug.Log($"Attempting to SELL {amount} of item ID {itemID} for {total}");

                bool success = Inventory.Instance.RemoveItem(itemID, amount);
                if (success)
                {
                    PlayerStats.Instance.Money += total;
                    Debug.Log($"Sold {amount} of item ID {itemID}. New balance: {PlayerStats.Instance.Money}");
                }
                else
                {
                    Debug.LogWarning($"Unexpected failure removing item ID {itemID}");
                }
            }
        }
    }



    private void EndTradingInteraction()
    {
        isTrading = false;

        // Clear cell UI elements
        foreach (Transform child in contentHolder)
        {
            Destroy(child.gameObject);
        }

        activeCells.Clear();

        if (tradingDisplay != null)
            tradingDisplay.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        NPCInteractionSystem.Instance.EndInteraction();
    }

}
