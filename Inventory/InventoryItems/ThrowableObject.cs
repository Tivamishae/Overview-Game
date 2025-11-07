using UnityEngine;

public class ThrowableObject : InventoryItemInfo
{
    public float Weight;
    public float ThrowingSpeed;
    public GameObject ThrownObject;
    public Vector3 rotationOffset; // NEW: Rotation offset applied on instantiation
}
