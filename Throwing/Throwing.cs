using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileThrower : MonoBehaviour
{
    public Transform projectileSpawnPoint;
    public float throwForce = 30f;
    public int equippedID;
    public GameObject equipped;

    public void Throwing()
    {
        equippedID = HotbarLogic.Instance.itemEquippedInHotbarID;
        equipped = ItemDatabase.Instance.Get2DPrefabByID(equippedID);

        if (equipped == null)
        {
            Debug.LogWarning("No item equipped to throw.");
            return;
        }

        ThrowableObject throwable = equipped.GetComponent<ThrowableObject>();
        if (throwable == null)
        {
            Debug.Log("Equipped item is not throwable.");
            return;
        }

        DragAndDrop drag = equipped.GetComponent<DragAndDrop>();
        if (drag == null)
        {
            Debug.LogWarning("Throwable item has no DragAndDrop component.");
            return;
        }

        int itemID = drag.itemID;
        GameObject projectilePrefab = ItemDatabase.Instance.GetProjectilePrefabByID(itemID);
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"No projectile found for itemID {itemID}.");
            return;
        }

        foreach (Transform t in Inventory.Instance.hotbarSlotsParent)
        {
            InventorySlot slot = t.GetComponent<InventorySlot>();
            if (slot != null && slot.itemID == itemID && slot.itemCount > 0)
            {
                foreach (Transform child in slot.transform)
                {
                    DragAndDrop d = child.GetComponent<DragAndDrop>();
                    if (d != null && d.itemID == itemID)
                    {
                        Destroy(child.gameObject);
                        break;
                    }
                }

                slot.UpdateSlotData();

                if (slot.itemCount <= 0)
                {
                    HotbarLogic.Instance.UnequipItem();
                }
                break;
            }
        }

        Vector3 spawnPos = projectileSpawnPoint.position;
        Quaternion spawnRot = projectileSpawnPoint.rotation;
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, spawnRot);

        StickOnHit stickScript = projectile.GetComponent<StickOnHit>();
        if (stickScript != null)
        {
            stickScript.itemID = itemID;
            projectile.transform.rotation *= stickScript.rotationOffset;
        }

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(projectileSpawnPoint.forward * throwForce, ForceMode.Impulse);
        }

        Debug.Log($"Threw item with itemID {itemID}");

        StartCoroutine(SlotUpdateDelayHotbar());
        HotbarLogic.Instance.UnequipItem();
    }

    public void Shooting()
    {
        equippedID = HotbarLogic.Instance.itemEquippedInHotbarID;
        equipped = ItemDatabase.Instance.Get2DPrefabByID(equippedID);

        if (equipped == null)
        {
            Debug.LogWarning("No item equipped to shoot.");
            return;
        }

        ProjectileShooter shooter = equipped.GetComponent<ProjectileShooter>();
        if (shooter == null)
        {
            Debug.Log("Equipped item is not a projectile shooter.");
            return;
        }

        if (!TryFindAmmo(shooter.allowedAmmoIDs, out int ammoID))
        {
            Debug.LogWarning("No valid ammo found in inventory.");
            return;
        }

        GameObject projectilePrefab = ItemDatabase.Instance.GetProjectilePrefabByID(ammoID);
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"No projectile prefab found for ammo ID {ammoID}");
            return;
        }

        Vector3 spawnPos = projectileSpawnPoint.position;
        Quaternion spawnRot = projectileSpawnPoint.rotation;
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, spawnRot);

        StickOnHit stickScript = projectile.GetComponent<StickOnHit>();
        if (stickScript != null)
        {
            stickScript.itemID = ammoID;
            projectile.transform.rotation *= stickScript.rotationOffset;
        }

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(projectileSpawnPoint.forward * shooter.ProjectileSpeed, ForceMode.Impulse);
        }

        Inventory.Instance.RemoveItem(ammoID, 1);

        StartCoroutine(SlotUpdateDelayInventory());

        Debug.Log($"Shot ammo with itemID {ammoID}");
    }

    private IEnumerator SlotUpdateDelayInventory()
    {
        yield return null; // Wait one frame

        foreach (InventorySlot slot in Inventory.Instance.inventorySlots)
        {
            slot.UpdateSlotData();
        }
    }

    private IEnumerator SlotUpdateDelayHotbar()
    {
        yield return null; // Wait one frame

        foreach (InventorySlot slot in Inventory.Instance.hotbarSlots)
        {
            slot.UpdateSlotData();
        }
    }

    private bool TryFindAmmo(List<int> ammoIDs, out int foundAmmoID)
    {
        foreach (int ammoID in ammoIDs)
        {
            foreach (var slot in Inventory.Instance.inventorySlots)
            {
                if (slot.itemID == ammoID && slot.itemCount > 0)
                {
                    foundAmmoID = ammoID;
                    return true;
                }
            }
        }

        foundAmmoID = -1;
        return false;
    }
}
