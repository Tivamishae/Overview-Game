using UnityEngine;

public class MineableObject : InteractableObject
{
    public float health = 3f;

    [Tooltip("Items allowed to mine this object. Use item IDs.")]
    public int[] allowedToolIDs;

    public string HitSound;

    [System.Serializable]
    public class DropData
    {
        public int itemID;
        public int amount = 1;
        [Range(0f, 1f)] public float dropChance = 1f;
    }

    public DropData[] drops;

    public override void Interact()
    {
        int equippedID = HotbarLogic.Instance.itemEquippedInHotbarID;

        if (!IsToolAllowed(equippedID))
        {
            Debug.Log($"Equipped item {equippedID} is not allowed to mine this object.");
            return;
        }

        Hit(1f); // Use default damage, or later retrieve it from equipped tool
        AudioClip hitSound = Resources.Load<AudioClip>("Sounds/InteractableObjects/" + HitSound);
        AudioSystem.Instance.PlayClipAtPoint(hitSound, this.transform.position, 1f);
    }

    private bool IsToolAllowed(int id)
    {
        if (allowedToolIDs.Length == 0)
            return true;

        foreach (int allowedID in allowedToolIDs)
        {
            if (allowedID == id)
                return true;
        }
        return false;
    }

    public void Hit(float damage)
    {
        health -= damage;

        if (health <= 0f)
        {
            DropItems();
            Destroy(gameObject);
        }
    }

    private void DropItems()
    {
        foreach (var drop in drops)
        {
            if (Random.value <= drop.dropChance)
            {
                Inventory.Instance.AddItem(drop.itemID, drop.amount);
                UISystem.Instance.ShowItemAddedPopup(drop.itemID, drop.amount);
            }
        }
    }
}
