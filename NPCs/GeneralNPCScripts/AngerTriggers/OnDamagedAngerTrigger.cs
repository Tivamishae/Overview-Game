using UnityEngine;

public class OnDamagedAngerTrigger : MonoBehaviour, IAngerTrigger
{
    private bool hasTriggered = false;

    public bool ShouldTrigger(NPC npc)
    {
        if (hasTriggered) return false;
        if (!npc.wasDamaged) return false;

        hasTriggered = true;
        return true;
    }
}
