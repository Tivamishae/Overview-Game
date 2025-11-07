using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class StorageCellScript : MonoBehaviour
{
    public int itemID;
    public int amount;

    private Button button;
    private TextMeshProUGUI amountText;
    private Image icon;
    public Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"{name} could not find Player with tag 'Player'");
        }
    }

    private bool isStorageSide; // true if from storage  inventory, false if inventory  storage

    public void Setup(int id, int amt, Sprite sprite, bool fromStorage)
    {
        itemID = id;
        amount = amt;
        isStorageSide = fromStorage;

        if (icon == null)
            icon = transform.Find("Icon")?.GetComponent<Image>();
        if (amountText == null)
            amountText = transform.Find("Amount")?.GetComponent<TextMeshProUGUI>();
        if (button == null)
            button = GetComponent<Button>();

        if (icon != null)
            icon.sprite = sprite;
        if (amountText != null)
            amountText.text = amount.ToString();
        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (isStorageSide)
        {
            // Move from storage to inventory
            if (Inventory.Instance.AddItem(itemID, 1))
            {
                StorageObjectSystem.Instance.currentStorage.RemoveStoredItem(itemID, 1);
                StorageObjectSystem.Instance.AddInventoryItem(itemID, 1);
                StorageObjectSystem.Instance.RefreshDisplay();
            }
        }
        else
        {
            StartCoroutine(RemoveAndRefreshCoroutine(itemID));
        }
        AudioClip hitSound = Resources.Load<AudioClip>("Sounds/UI/TransferItems");
        AudioSystem.Instance.PlayClipAtPoint(hitSound, player.position, 1f);
    }

    private IEnumerator RemoveAndRefreshCoroutine(int itemID)
    {

        // Wait 1 frame so Destroy() takes effect
        yield return null;

        if (Inventory.Instance.RemoveItem(itemID, 1))
        {
            StorageObjectSystem.Instance.currentStorage.AddStoredItem(itemID, 1);
            StorageObjectSystem.Instance.RemoveInventoryItem(itemID, 1);
            StorageObjectSystem.Instance.RefreshDisplay();
        }
    }
}
