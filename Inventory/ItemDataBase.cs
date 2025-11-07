using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    private Dictionary<int, GameObject> item2DPrefabMap = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> item3DPrefabMap = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> itemEquipmentItem = new Dictionary<int, GameObject>();
    private Dictionary<int, Sprite> itemSpriteMap = new Dictionary<int, Sprite>();
    private Dictionary<int, GameObject> itemProjectileMap = new Dictionary<int, GameObject>();


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadAllInventoryPrefabs();
        LoadAllWorldPrefabs();
        LoadAllEquipmentPrefabs();
        LoadAllItemSprites();
        LoadAllProjectilePrefabs();
    }

    void LoadAllInventoryPrefabs()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("2D/InventoryItems");

        foreach (GameObject prefab in loadedPrefabs)
        {
            DragAndDrop drag = prefab.GetComponent<DragAndDrop>();
            if (drag != null)
            {
                int id = drag.itemID;
                if (!item2DPrefabMap.ContainsKey(id))
                    item2DPrefabMap.Add(id, prefab);
                else
                    Debug.LogWarning($"Duplicate 2D itemID {id} on prefab {prefab.name} — skipping.");
            }
        }
    }

    void LoadAllWorldPrefabs()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("3D/Prefabs");

        foreach (GameObject prefab in loadedPrefabs)
        {
            InteractableObject obj = prefab.GetComponent<InteractableObject>();
            if (obj != null)
            {
                int id = obj.ItemID;
                if (!item3DPrefabMap.ContainsKey(id))
                    item3DPrefabMap.Add(id, prefab);
                else
                    Debug.LogWarning($"Duplicate 3D itemID {id} on prefab {prefab.name} — skipping.");
            }
        }
    }

    void LoadAllEquipmentPrefabs()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("3D/Equipment");

        foreach (GameObject prefab in loadedPrefabs)
        {
            EquipmentItem equip = prefab.GetComponent<EquipmentItem>();
            if (equip != null)
            {
                int id = equip.itemID;
                if (!itemEquipmentItem.ContainsKey(id))
                    itemEquipmentItem.Add(id, prefab);
                else
                    Debug.LogWarning($"Duplicate equipment itemID {id} on prefab {prefab.name} — skipping.");
            }
        }
    }

    void LoadAllItemSprites()
    {
        Sprite[] loadedSprites = Resources.LoadAll<Sprite>("2D/InventoryItemImages");

        foreach (Sprite sprite in loadedSprites)
        {
            string spriteName = sprite.name;

            foreach (var kvp in item2DPrefabMap)
            {
                GameObject prefab = kvp.Value;
                if (prefab.name == spriteName)
                {
                    int itemID = kvp.Key;

                    if (!itemSpriteMap.ContainsKey(itemID))
                    {
                        itemSpriteMap.Add(itemID, sprite);
                        Debug.Log($"Linked sprite '{spriteName}' to itemID {itemID}");
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate sprite match for itemID {itemID} — skipping.");
                    }

                    break; // Found match; no need to keep comparing
                }
            }
        }
    }

    void LoadAllProjectilePrefabs()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("3D/Projectiles");

        foreach (GameObject prefab in loadedPrefabs)
        {
            StickOnHit projectile = prefab.GetComponent<StickOnHit>(); // or another script, see note below
            if (projectile != null)
            {
                int id = projectile.itemID;
                if (!itemProjectileMap.ContainsKey(id))
                    itemProjectileMap.Add(id, prefab);
                else
                    Debug.LogWarning($"Duplicate projectile itemID {id} on prefab {prefab.name} — skipping.");
            }
            else
            {
                Debug.LogWarning($"Prefab {prefab.name} in Projectiles has no EquipmentItem component — skipping.");
            }
        }
    }


    public GameObject Get2DPrefabByID(int id)
    {
        if (item2DPrefabMap.TryGetValue(id, out GameObject prefab))
            return prefab;

        Debug.LogWarning($"No 2D prefab found with itemID {id}.");
        return null;
    }

    public GameObject Get3DPrefabByID(int id)
    {
        if (item3DPrefabMap.TryGetValue(id, out GameObject prefab))
            return prefab;

        Debug.LogWarning($"No 3D prefab found with itemID {id}.");
        return null;
    }

    public GameObject GetEquipmentPrefabByID(int id)
    {
        if (itemEquipmentItem.TryGetValue(id, out GameObject prefab))
            return prefab;

        Debug.LogWarning($"No equipment prefab found with itemID {id}.");
        return null;
    }

    public Sprite GetItemSpriteByID(int id)
    {
        if (itemSpriteMap.TryGetValue(id, out Sprite sprite))
            return sprite;

        Debug.LogWarning($"No sprite found with itemID {id}.");
        return null;
    }

    public GameObject GetProjectilePrefabByID(int id)
    {
        if (itemProjectileMap.TryGetValue(id, out GameObject sprite))
            return sprite;

        Debug.LogWarning($"No Projectile found with itemID {id}.");
        return null;
    }

    public int GetItemPriceByID(int itemID)
{
    GameObject prefab = Get2DPrefabByID(itemID);
    if (prefab != null)
    {
        InventoryItemInfo info = prefab.GetComponent<InventoryItemInfo>();
        if (info != null)
        {
            return info.Price;
        }
    }

    Debug.LogWarning($"Price not found for itemID {itemID}. Defaulting to 1.");
    return 1; // fallback
}

}
