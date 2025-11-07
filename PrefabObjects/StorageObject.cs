using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StoredItem
{
    public int itemID;
    public int amount;
}

[System.Serializable]
public class PossibleLoot
{
    public int itemID;
    public int amount;
    public float chance;
}

public class StorageObject : InteractableObject
{
    [Header("Storage Settings")]
    public bool DestroyOnEmpty = true;  // new toggle
    public List<StoredItem> storedItems = new(); // Populate this per chest in Inspector

    public override void Interact()
    {
        StorageObjectSystem.Instance.OpenStorage(this);
    }

    public void AddStoredItem(int id, int amt)
    {
        StoredItem found = storedItems.Find(s => s.itemID == id);
        if (found != null)
            found.amount += amt;
        else
            storedItems.Add(new StoredItem { itemID = id, amount = amt });
    }

    public void RemoveStoredItem(int id, int amt)
    {
        StoredItem found = storedItems.Find(s => s.itemID == id);
        if (found != null)
        {
            found.amount -= amt;
            if (found.amount <= 0)
                storedItems.Remove(found);
        }

        CheckEmpty(); //  check after removing
    }

    private void CheckEmpty()
    {
        if (DestroyOnEmpty && storedItems.Count == 0)
        {
            // Close UI
            if (StorageObjectSystem.Instance != null && StorageObjectSystem.Instance.storageDisplay != null)
                StorageObjectSystem.Instance.storageDisplay.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Destroy self
            Destroy(gameObject);
        }
    }
}
