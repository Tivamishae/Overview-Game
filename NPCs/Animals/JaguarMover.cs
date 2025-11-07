using UnityEngine;
using UnityEngine.AI;

public class JaguarMover : CreatureMover
{
    [Header("Behaviour Distances")]
    public float stalkDistance = 30f;
    public float pounceDistance = 15f;

    [Header("Combat Behaviour")]
    public float commitTime = 4f; // Seconds to keep attacking even if spotted
    public float damageBreakPercent = 60f; // Break off if takes this  of max HP during commit
    public float cooldownAfterRetreat = 8f;

    private enum JaguarState { Wandering, Stalking, Attacking, Retreating, Cooldown }
    private JaguarState state = JaguarState.Wandering;

    private float commitTimer;
    private float cooldownTimer;
    private float initialHealth;

    public void Start()
    {
        initialHealth = Health;
    }

    public override void Update()
    {
        if (isDead) return;

        switch (state)
        {
            case JaguarState.Wandering:
                if (CheckPlayerView())
                {
                    Retreat();
                }
                else
                {
                    HandleWandering();
                }
                break;

            case JaguarState.Stalking:
                HandleStalking();
                break;

            case JaguarState.Attacking:
                HandleAttacking();
                break;

            case JaguarState.Retreating:
                HandleRetreating();
                break;

            case JaguarState.Cooldown:
                HandleCooldown();
                break;
        }
    }




    // -------------------------
    // Core Behaviour Functions
    // -------------------------
    private void HandleWandering()
    {
        if (CanStalk())
        {
            state = JaguarState.Stalking;
        }
        else
        {
            base.Update(); // Default CreatureMover wander
        }
    }

    private void HandleStalking()
    {
        if (!CanStalk())
        {
            state = JaguarState.Wandering;
            return;
        }

        float distance = Vector3.Distance(player.position, transform.position);

        if (CheckPlayerView())
        {
            // If close enough but not in pounce range, commit to attack
            if (distance <= stalkDistance / 2f && distance > pounceDistance)
            {
                BeginAttack();
                return;
            }
            else
            {
                Retreat();
                return;
            }
        }

        if (distance <= pounceDistance)
        {
            BeginAttack();
        }
        else
        {
            agent.isStopped = false;
            agent.speed = WalkSpeed;
            agent.SetDestination(player.position);
            animationMover?.SetIsMoving(true);
            animationMover?.SetIsRunning(false);
        }
    }

    private void HandleAttacking()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        // Break if too much damage taken
        float damagePercent = ((initialHealth - Health) / initialHealth) * 100f;
        if (damagePercent >= damageBreakPercent)
        {
            Retreat();
            return;
        }

        // Out of pounce range  go back to stalking
        if (distance > pounceDistance)
        {
            state = JaguarState.Stalking;
            animationMover?.SetIsAttacking(false);
            return;
        }

        // In attack range  stop and attack
        if (distance <= attackRange)
        {
            agent.isStopped = true;
            animationMover?.SetIsMoving(false);
            animationMover?.SetIsRunning(false);
            animationMover?.SetIsAttacking(true);
        }
        else
        {
            agent.isStopped = false;
            agent.speed = RunningSpeed;
            agent.SetDestination(player.position);
            animationMover?.SetIsMoving(true);
            animationMover?.SetIsRunning(true);
            animationMover?.SetIsAttacking(false);
        }
    }


    private void HandleRetreating()
    {
        float currentDist = Vector3.Distance(player.position, transform.position);

        // If already far enough, switch to cooldown
        if (currentDist >= 50f)
        {
            cooldownTimer = cooldownAfterRetreat;
            state = JaguarState.Cooldown;
            return;
        }

        // Keep running away
        Vector3 fleeDir = (transform.position - player.position).normalized;
        Vector3 targetPos = transform.position + fleeDir * 10f; // step away 10f at a time

        // Use NavMesh to get a valid point
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.speed = RunningSpeed;
            agent.SetDestination(hit.position);
        }

        animationMover?.SetIsMoving(true);
        animationMover?.SetIsRunning(true);
    }


    private void HandleCooldown()
    {
        float distToPlayer = Vector3.Distance(player.position, transform.position);

        // If player is too close, keep retreating
        if (distToPlayer < 25f) // safety threshold
        {
            state = JaguarState.Retreating;
            return;
        }

        cooldownTimer -= Time.deltaTime;
        base.Update(); // wander during cooldown

        if (cooldownTimer <= 0f)
        {
            state = JaguarState.Wandering;
        }
    }


    // -------------------------
    // Utility / Action Methods
    // -------------------------
    private bool CheckPlayerView()
    {
        Vector3 toJaguar = (transform.position - player.position).normalized;
        float dot = Vector3.Dot(player.forward, toJaguar);
        return dot > 0.5f; // true if player facing jaguar
    }

    private bool CanStalk()
    {
        return !CheckPlayerView() &&
               Vector3.Distance(player.position, transform.position) <= stalkDistance;
    }

    private void BeginAttack()
    {
        state = JaguarState.Attacking;
        commitTimer = commitTime;
        initialHealth = Health;
        angry = true;
    }

    private void Retreat()
    {
        state = JaguarState.Retreating;
        animationMover?.SetIsAttacking(false);
        angry = false;
    }

    // Animation event hook
    public void Attack()
    {
        if (Vector3.Distance(player.position, transform.position) <= attackRange)
        {
            PlayerStats.Instance?.TakeDamage(15f); // Example damage

            // Play jaguar attack sound
            AudioClip attackClip = Resources.Load<AudioClip>("Sounds/Creatures/Jaguar/Attack");
            if (attackClip != null)
            {
                AudioSystem.Instance.PlayClipFollow(attackClip, transform, 1f);
            }
            else
            {
                Debug.LogWarning("Jaguar attack sound not found at: Resources/Sounds/Creatures/Jaguar/Attack");
            }
        }
    }

}
