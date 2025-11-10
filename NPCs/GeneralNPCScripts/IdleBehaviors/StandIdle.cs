using UnityEngine;

public class StandIdle : MonoBehaviour, IIdleReaction
{
    public void ExecuteIdle(NPC npc)
    {
        npc.StopMoving();
        npc.PlayBool("Idle", true);
    }
}
