using UnityEngine;

public abstract class HumanInteraction : MonoBehaviour
{
    public abstract void Interact(); // Initial trigger
    public virtual void Progress() { } // Optional: for things like continuing conversation
    public virtual void OpenLoot()
    {

    }
}