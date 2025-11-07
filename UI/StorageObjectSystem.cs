using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class DisplayedItem
{
    public int itemID;
    public int amount;
}

public class StorageObjectSystem : MonoBehaviour
{
    public static StorageObjectSystem Instance { get; private set; }

    public GameObject storageDisplay;
    public StorageObject currentStorage;

    public Transform inventoryContentHolder;
    public Transform storageContentHolder;
    public GameObject storageCellPrefab;

    public GameObject inventorySlotsObject; // <- Assign in Inspector (the invisible inventory slot container)

    public List<DisplayedItem> displayedInventoryItems = new();

    private bool storageIsOpened = false;
    private Transform playerTransform;
    public float maxDistance = 5f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (storageDisplay == null)
        {
            storageDisplay = GameObject.Find("UI/Popups/StorageObjectDisplay");
            if (storageDisplay == null)
            {
                Debug.LogWarning("StorageObjectDisplay not found!");
                return;
            }
        }

        if (inventoryContentHolder == null)
        {
            inventoryContentHolder = storageDisplay.transform.Find("Inventory/ItemHolder/ViewPort/Content");
            if (inventoryContentHolder == null)
                Debug.LogWarning("Inventory content holder not found.");
        }

        if (storageContentHolder == null)
        {
            storageContentHolder = storageDisplay.transform.Find("Storage/ItemHolder/ViewPort/Content");
            if (storageContentHolder == null)
                Debug.LogWarning("Storage content holder not found.");
        }

        if (storageCellPrefab == null)
        {
            storageCellPrefab = Resources.Load<GameObject>("2D/InventoryDisplays/StorageCell");
            if (storageCellPrefab == null)
            {
                Debug.LogWarning("StorageCell prefab not found in Resources.");
            }
        }

        storageDisplay.SetActive(false);
    }

    void Update()
    {
        if (!storageIsOpened)
        {
            displayedInventoryItems.Clear();
            foreach (var slot in Inventory.Instance.inventorySlots)
            {
                if (slot.itemID != -1 && slot.itemCount > 0)
                {
                    AddInventoryItem(slot.itemID, slot.itemCount);
                }
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            CloseStorage();
            return;
        }

        if (playerTransform != null && currentStorage != null)
        {
            float distance = Vector3.Distance(playerTransform.position, currentStorage.transform.position);
            if (distance > maxDistance)
            {
                CloseStorage();
            }
        }
    }

    public void OpenStorage(StorageObject storage)
    {
        if (storageDisplay == null || storageCellPrefab == null ||
            inventoryContentHolder == null || storageContentHolder == null)
            return;

        currentStorage = storage;
        storageIsOpened = true;
        storageDisplay.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PopulateStorageContent();
    }

    private void PopulateStorageContent()
    {
        foreach (Transform child in storageContentHolder)
            Destroy(child.gameObject);

        foreach (Transform child in inventoryContentHolder)
            Destroy(child.gameObject);

        // Populate STORAGE side
        foreach (StoredItem item in currentStorage.storedItems)
        {
            GameObject cell = Instantiate(storageCellPrefab, storageContentHolder);
            Sprite sprite = ItemDatabase.Instance.GetItemSpriteByID(item.itemID);
            cell.GetComponent<StorageCellScript>().Setup(item.itemID, item.amount, sprite, true);
        }

        // Populate INVENTORY side
        foreach (var item in displayedInventoryItems)
        {
            GameObject cell = Instantiate(storageCellPrefab, inventoryContentHolder);
            Sprite sprite = ItemDatabase.Instance.GetItemSpriteByID(item.itemID);
            cell.GetComponent<StorageCellScript>().Setup(item.itemID, item.amount, sprite, false);
        }
    }

    public void RefreshDisplay()
    {
        PopulateStorageContent();
    }

    public void CloseStorage()
    {
        StartCoroutine(StorageCloseTimer());
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator StorageCloseTimer()
    {
        yield return null;

        // Temporarily show inventory slots object for 1 frame, make it invisible
        if (inventorySlotsObject != null)
        {
            CanvasGroup cg = inventorySlotsObject.GetComponent<CanvasGroup>();
            bool added = false;

            if (cg == null)
            {
                cg = inventorySlotsObject.AddComponent<CanvasGroup>();
                added = true;
            }

            cg.alpha = 0f;
            inventorySlotsObject.SetActive(true);

            yield return new WaitForSeconds(0.1f);

            inventorySlotsObject.SetActive(false);

            if (added)
                Destroy(cg); // Clean up if we added it ourselves
        }

        storageDisplay.SetActive(false);
        storageIsOpened = false;
        currentStorage = null;
    }

    public void AddInventoryItem(int id, int amt)
    {
        var found = displayedInventoryItems.Find(i => i.itemID == id);
        if (found != null)
            found.amount += amt;
        else
            displayedInventoryItems.Add(new DisplayedItem { itemID = id, amount = amt });
    }

    public void RemoveInventoryItem(int id, int amt)
    {
        var found = displayedInventoryItems.Find(i => i.itemID == id);
        if (found != null)
        {
            found.amount -= amt;
            if (found.amount <= 0)
                displayedInventoryItems.Remove(found);
        }
    }

    public bool IsStorageOpen() => storageIsOpened;
}
