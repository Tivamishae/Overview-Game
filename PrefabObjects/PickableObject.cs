using UnityEngine;

public class PickableObject : InteractableObject
{

    public override void Interact()
    {
        bool added = Inventory.Instance.AddItem(ItemID, 1);
        if (added)
        {
            UISystem.Instance.ShowItemAddedPopup(ItemID, 1);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Inventory full!");
        }
    }
}