using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UISystem : MonoBehaviour
{
    public static UISystem Instance { get; private set; }

    public GameObject Inventory;
    public GameObject InventorySlots;
    public GameObject WorkableInventory;
    public GameObject Pointer;
    public GameObject ObjectName;
    public Camera PlayerCamera;
    public GameObject MissingMaterialsPopup;

    [Header("Quest UI")]
    public GameObject QuestScreen; // Assign same object as QuestSystem.questScreen

    [Header("Item Added Popup")]
    public GameObject ItemAddedPopup; // Assign a popup with Image + Text child
    public CanvasGroup popupCanvasGroup;
    public Coroutine popupRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        Inventory.SetActive(false);
        WorkableInventory.SetActive(false);
        ObjectName.SetActive(false);
        MapSystem.Instance.Map.SetActive(false);
        QuestScreen.SetActive(false);

        if (ItemAddedPopup != null)
        {
            popupCanvasGroup = ItemAddedPopup.GetComponent<CanvasGroup>();
            if (popupCanvasGroup == null)
                popupCanvasGroup = ItemAddedPopup.AddComponent<CanvasGroup>();

            popupCanvasGroup.alpha = 0f;
            ItemAddedPopup.SetActive(false);
        }
    }

    void Update()
    {
        // Inventory toggle (Tab)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool inventoryNowActive = !Inventory.activeSelf;

            Inventory.SetActive(inventoryNowActive);
            WorkableInventory.SetActive(inventoryNowActive);
            MissingMaterialsPopup.SetActive(false);
            QuestScreen.SetActive(false);
            MapSystem.Instance.Map.SetActive(false);

            ApplyCursorAndPointerState();
        }

        // Quest screen toggle (Q)
        if (Input.GetKeyDown(KeyCode.Q) && QuestScreen != null)
        {
            QuestScreen.SetActive(!QuestScreen.activeSelf);
            QuestSystem.Instance.RefreshQuestUI(QuestSystem.Instance.quests, true);
            // Optional: ensure inventory closes when opening quest, to avoid overlap
            if (QuestScreen.activeSelf)
            {
                Inventory.SetActive(false);
                WorkableInventory.SetActive(false);
                MissingMaterialsPopup.SetActive(false);
                MapSystem.Instance.Map.SetActive(false);
            }

            ApplyCursorAndPointerState();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            bool MapNowActive = !MapSystem.Instance.Map.activeSelf;
            MapSystem.Instance.Map.SetActive(MapNowActive);
            MapSystem.Instance.renderMap = MapNowActive;
            MissingMaterialsPopup.SetActive(false);
            QuestScreen.SetActive(false);

            ApplyCursorAndPointerState();
        }
        if (Inventory.activeSelf == false && QuestSystem.Instance.questScreen.activeSelf == false  && QuestSystem.Instance.currentQuest != null)
        {
            QuestSystem.Instance.CurrentQuestPopup.SetActive(true);
            QuestSystem.Instance.RefreshCurrentQuestPopup();
        }
        else
        {
            QuestSystem.Instance.CurrentQuestPopup.SetActive(false);
        }
    }

    /// <summary>
    /// Central place to decide cursor lock/visibility and pointer visibility
    /// based on whether *any* UI is open.
    /// </summary>
    private void ApplyCursorAndPointerState()
    {
        bool anyUiOpen =
            (Inventory != null && Inventory.activeSelf) ||
            (WorkableInventory != null && WorkableInventory.activeSelf) ||
            (QuestScreen != null && QuestScreen.activeSelf);

        Cursor.lockState = anyUiOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = anyUiOpen;
        if (Pointer != null) Pointer.SetActive(!anyUiOpen);
    }

    public void ShowItemAddedPopup(int itemID, int amount)
    {
        if (ItemAddedPopup == null) return;

        // Get Image and Text children
        Image image = ItemAddedPopup.transform.GetChild(0).GetComponent<Image>();
        TextMeshProUGUI text = ItemAddedPopup.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        // Get sprite and name from database
        Sprite itemSprite = ItemDatabase.Instance.GetItemSpriteByID(itemID);
        GameObject prefab = ItemDatabase.Instance.Get2DPrefabByID(itemID);

        if (prefab != null)
        {
            string itemName = prefab.name;
            text.text = $"+{amount} {itemName}";
        }
        else
        {
            text.text = $"+{amount} Unknown Item";
        }

        if (itemSprite != null)
        {
            image.sprite = itemSprite;
            image.color = Color.white;
        }
        else
        {
            image.sprite = null;
            image.color = Color.clear;
        }

        if (popupRoutine != null)
            StopCoroutine(popupRoutine);

        popupRoutine = StartCoroutine(HandleItemPopup());
    }

    private IEnumerator HandleItemPopup()
    {
        ItemAddedPopup.SetActive(true);
        popupCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(3f);

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            popupCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        popupCanvasGroup.alpha = 0f;
        ItemAddedPopup.SetActive(false);
        popupRoutine = null;
    }
}
