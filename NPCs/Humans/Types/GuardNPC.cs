using UnityEngine;
using UnityEngine.AI;

public class GuardNPC : InteractableNPC
{
    [Header("Guard Settings")]
    public float alertRadius = 50f;
    public float checkInterval = 1f;

    private float checkTimer = 0f;

    void Update()
    {
        if (isDead) return;

        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckForAngryNPCs();
        }

        if (angry)
        {
            ChasePlayer();
        }
    }

    private void CheckForAngryNPCs()
    {
        if (angry) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, alertRadius);
        foreach (var col in colliders)
        {
            InteractableNPC npc = col.GetComponent<InteractableNPC>();

            // If we detect ANY angry NPC that is not a guard, we become angry and chase player
            if (npc != null && npc != this && npc.angry && !npc.isDead)
            {
                Debug.Log($"{name} becomes angry after sensing {npc.name}!");
                TriggerAnger();
                return;
            }
        }
    }

    private void ChasePlayer()
    {
        if (!player || !agent) return;

        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.SetDestination(player.transform.position);

        // Animation hooks
        humanMover.SetIsRunning(true);
        humanMover.SetIsMoving(true);
        humanMover.SetIsAngry(true);

        FaceTarget(player.transform.position);
    }
}
