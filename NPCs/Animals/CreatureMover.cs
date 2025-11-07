using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class CreatureMover : MonoBehaviour
{
    [Header("Stats")]
    public float WalkSpeed = 3.5f;
    public float RunningSpeed = 6f;
    public float JumpHeight = 2f;
    public float Health = 100f;
    public bool angry = false;

    [Header("Attack Settings")]
    public float attackRange = 2.5f;
    public float attackDamage = 15f;
    public float attackHitRange = 3f;

    public string Name;

    [Header("Wandering")]
    public float wanderRadius = 10f;
    public float walkTime = 3f;
    public float idleTime = 2f;
    public float stateTimer;
    private Vector3 startPosition;
    public bool isRunning = false;

    public enum WanderState { Idle, Moving }
    public WanderState currentState = WanderState.Idle;

    [Header("References")]
    public NavMeshAgent agent;
    public CreatureAnimationMover animationMover;


    [Header("Target")]
    public Transform player;

    public bool isDead = false;
    public List<StoredItem> loot = new();
    public List<PossibleLoot> possibleLoot = new();

    public string HitSound;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;

        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"{name} could not find Player with tag 'Player'");
        }

        currentState = WanderState.Idle;
        stateTimer = idleTime;

        if (animationMover != null)
        {
            animationMover.SetIsMoving(false);
            animationMover.SetIsRunning(false);
        }
    }

    public virtual void Update()
    {
        if (isDead) return; // No movement if dead

        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case WanderState.Idle:
                agent.isStopped = true;
                animationMover?.SetIsMoving(false);
                animationMover?.SetIsRunning(false);

                if (stateTimer <= 0f)
                {
                    agent.speed = isRunning ? RunningSpeed : WalkSpeed;
                    animationMover?.SetIsMoving(true);
                    animationMover?.SetIsRunning(isRunning);

                    Vector3 newPos = RandomNavSphere(startPosition, wanderRadius, NavMesh.AllAreas);
                    agent.SetDestination(newPos);
                    agent.isStopped = false;

                    currentState = WanderState.Moving;
                    stateTimer = walkTime;
                }
                break;

            case WanderState.Moving:
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    stateTimer = 0f; // Arrived early
                }

                if (stateTimer <= 0f)
                {
                    currentState = WanderState.Idle;
                    stateTimer = idleTime;
                    agent.isStopped = true;
                    animationMover?.SetIsMoving(false);
                    animationMover?.SetIsRunning(false);
                }
                break;
        }
    }

    private Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * dist;
        randomDirection += origin;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, dist, layermask))
        {
            return navHit.position;
        }
        return origin;
    }

    public void Attack()
    {
        if (player == null || PlayerStats.Instance == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackHitRange)
        {
            PlayerStats.Instance.TakeDamage(attackDamage);
            Debug.Log($"{Name} hit the player for {attackDamage} damage.");
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        Health -= damage;
        animationMover?.PlayHitTrigger();

        AudioClip hitSound = Resources.Load<AudioClip>("Sounds/InteractableObjects/" + HitSound);
        AudioSystem.Instance.PlayClipAtPoint(hitSound, this.transform.position, 1f);

        // Stop movement briefly
        StartCoroutine(StopMovementTemporarily(0.2f));

        // Death check
        if (Health <= 0f)
        {
            Die();
        }
    }

    private IEnumerator StopMovementTemporarily(float seconds)
    {
        if (agent != null)
        {
            agent.isStopped = true;
            yield return new WaitForSeconds(seconds);
            if (!isDead) agent.isStopped = false;
        }
    }

    private void Die()
    {
        isDead = true;
        agent.isStopped = true;
        animationMover?.PlayDeathTrigger();
        Debug.Log($"{Name} has died.");
        ResetAllAnimatorBools();

        StartCoroutine(DelayedDeath());
    }

    public void ResetAllAnimatorBools()
    {
        if (animationMover == null || animationMover.animator == null)
            return;

        animationMover.SetIsMoving(false);
        animationMover.SetIsRunning(false);
        animationMover.SetIsAttacking(false);
    }

    private IEnumerator DelayedDeath()
    {
        yield return new WaitForSeconds(0.8f);

        // Roll loot from possibleLoot
        GenerateLoot();

        // Add StorageObject to this GameObject (the one wearing this script)
        StorageObject lootStorage = gameObject.AddComponent<StorageObject>();
        lootStorage.storedItems = new List<StoredItem>(loot);

        Instantiate(Resources.Load<GameObject>("3D/Effects/LootDropEffect"), transform);

        Destroy(GetComponent<CreatureMover>());
    }




    private void GenerateLoot()
    {
        loot.Clear(); // reset any old data

        foreach (var p in possibleLoot)
        {
            if (Random.value <= p.chance) // roll chance
            {
                // check if item already exists in loot
                StoredItem existing = loot.Find(x => x.itemID == p.itemID);
                if (existing != null)
                {
                    existing.amount += p.amount;
                }
                else
                {
                    loot.Add(new StoredItem { itemID = p.itemID, amount = p.amount });
                }
            }
        }
    }



}
