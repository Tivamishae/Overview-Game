using UnityEngine;
using System.Collections.Generic;

public class ProjectileShooter : InventoryItemInfo
{
    public float ProjectileWeight;
    public float ProjectileSpeed;
    public GameObject Projectile;
    public Vector3 rotationOffset;

    public List<int> allowedAmmoIDs; // IDs of valid ammo (e.g., [8, 9])
}
