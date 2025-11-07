using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public string Name;
    public int ItemID;

    public abstract void Interact();
}