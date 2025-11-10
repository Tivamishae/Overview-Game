using UnityEngine;

public class AlwaysAngryOnSeeTrigger : MonoBehaviour, IAngerTrigger
{
    private bool triggered;

    public bool ShouldTrigger(NPC npc)
    {
        if (triggered) return false;
        if (!npc.noticesPlayer) return false;

        triggered = true;
        return true;
    }
}
