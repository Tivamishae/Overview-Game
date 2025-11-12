using UnityEngine;

public class WanderIdle : MonoBehaviour, IIdleReaction
{
    public float wanderRadius = 10f;
    public float standStillTimeMax = 10f;
    public float standStillTimeMin = 0f;
    private float standStillTimer = 0f;
    private bool isStandingStill = true;

    public void ExecuteIdle(NPC npc)
    {
        if (isStandingStill)
        {
            standStillTimer -= Time.deltaTime;

            npc.PlayBool("Walking", false);
            npc.PlayBool("Idle", true);

            if (standStillTimer <= 0)
            {
                npc.SetNewWanderDestination(wanderRadius);
                isStandingStill = false;
                npc.PlayBool("Walking", true);
                npc.PlayBool("Idle", false);
            }
        }
        else
        {
            if (!npc.agent.pathPending && npc.agent.remainingDistance <= npc.agent.stoppingDistance)
            {
                isStandingStill = true;
                standStillTimer = Random.Range(standStillTimeMin, standStillTimeMax);
            }
        }
    }
}

