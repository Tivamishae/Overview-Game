using UnityEngine;

public class AngerReactions : MonoBehaviour
{
    public static AngerReactions Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void RunAwayReaction(NPC npc)
    {
        if (npc.player == null) return;
        npc.RunAwayFrom(npc.player.transform.position);
        npc.PlayBool("Running", true);
        npc.PlayBool("Idle", false);
    }
    public void ChaseReaction(NPC npc, float stopDistance = 2f)
    {
        if (npc.player == null) return;

        float distance = Vector3.Distance(npc.transform.position, npc.player.transform.position);

        if (distance > stopDistance)
        {
            npc.MoveTowards(npc.player.transform.position);
            npc.PlayBool("Running", true);
            npc.PlayBool("Idle", false);
        }
        else
        {
            npc.StopMoving();
            npc.PlayBool("Idle", true);
            npc.PlayBool("Running", false);
        }
    }
    public void KeepDistanceReaction(NPC npc, float idealDistance = 10f, float moveAwayDistance = 6f)
    {
        if (npc.player == null) return;

        float distance = Vector3.Distance(npc.transform.position, npc.player.transform.position);

        if (distance > idealDistance)
        {
            npc.MoveTowards(npc.player.transform.position);
            npc.PlayBool("Running", true);
            npc.PlayBool("Idle", false);
        }
        else if (distance < moveAwayDistance)
        {
            npc.RunAwayFrom(npc.player.transform.position);
            npc.PlayBool("Running", true);
            npc.PlayBool("Idle", false);
        }
        else
        {
            npc.StopMoving();
            npc.PlayBool("Idle", true);
            npc.PlayBool("Running", false);
        }
    }
    public void StandStillReaction(NPC npc)
    {
        if (npc.player == null) return;

        npc.StopMoving();
        npc.FaceTarget(npc.player.transform.position);
    }
}