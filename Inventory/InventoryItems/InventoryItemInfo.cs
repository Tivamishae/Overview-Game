using UnityEngine;

public class InventoryItemInfo : MonoBehaviour
{
    public string Name;
    public string Description;
    public string Functionality;

    public int MaxStackAmount;

    [Header("Shop Info")]
    public int Price;
}
