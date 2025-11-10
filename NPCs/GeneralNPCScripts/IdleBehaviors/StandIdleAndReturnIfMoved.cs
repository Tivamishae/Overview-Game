using UnityEngine;

public class StandIdleAndReturnIfMoved : MonoBehaviour, IIdleReaction
{
    public void ExecuteIdle(NPC npc)
    {
        if (npc.spawnPosition == npc.transform.position)
        {
            npc.StopMoving();
            npc.PlayBool("Idle", true);
            npc.PlayBool("Walking", false);
        }
        else
        {
            npc.MoveTowards(npc.spawnPosition);
            npc.PlayBool("Idle", false);
            npc.PlayBool("Walking", true);
        }
    }
}
