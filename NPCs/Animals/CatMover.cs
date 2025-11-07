using UnityEngine;

public class CatMover : CreatureMover
{
    private bool isFleeing = false;
    private float fleeStartTime;

    private void Start()
    {
        Name = "Cat";
        isRunning = false; // Cats walk normally
    }

    public override void Update()
    {
        if (isDead) return;

        if (isFleeing)
        {
            FleeUpdate();
        }
        else
        {
            // Default wandering logic
            base.Update();
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage); // Handles health & animations

        if (!isDead && player != null)
        {
            StartFleeing();
        }
    }

    private void StartFleeing()
    {
        isFleeing = true;
        isRunning = true;
        fleeStartTime = Time.time;
        agent.speed = RunningSpeed;
    }

    private void FleeUpdate()
    {
        if (player == null) return;

        // Pick a point away from the player
        Vector3 fleeDirection = (transform.position - player.position).normalized;
        Vector3 fleeTarget = transform.position + fleeDirection * wanderRadius * 2f;

        if (agent.isStopped) agent.isStopped = false;
        agent.SetDestination(fleeTarget);

        animationMover?.SetIsMoving(true);
        animationMover?.SetIsRunning(true);

        // Check exit conditions
        float distance = Vector3.Distance(transform.position, player.position);
        bool farEnough = distance >= 80f;
        bool timePassed = (Time.time - fleeStartTime) >= 60f;

        if (farEnough && timePassed)
        {
            StopFleeing();
        }
    }

    private void StopFleeing()
    {
        isFleeing = false;
        isRunning = false;
        agent.speed = WalkSpeed;

        // Reset back to wandering
        stateTimer = idleTime;
        currentState = (CreatureMover.WanderState)0; // Force Idle
        agent.isStopped = true;
        animationMover?.SetIsMoving(false);
        animationMover?.SetIsRunning(false);
    }
}
