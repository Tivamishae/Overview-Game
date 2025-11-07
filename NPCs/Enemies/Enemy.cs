using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct CombatAnimationData
{
    public string animation;
    public bool freezeRotation;
}

public class Enemy : MonoBehaviour
{
    [Header("General Enemy Settings")]
    public float maxHealth;
    public float currentHealth;
    public bool angry;
    public string Name;

    [Header("Movement Settings")]
    protected Vector3 spawnPosition;
    public float WanderRadius = 15f;
    public bool wanderCloseToSpawn = false;
    public float idleTimer;
    public float maxIdleCooldown = 10f;
    public float minIdleCooldown = 3f;
    public enum EnemyState { Idle, Wandering, Combat, Dead, Searching }
    public EnemyState currentState;
    protected bool canRotate = true;

    [Header("Combat Settings")]
    public float chaseTimer;
    protected bool playerInRange = false;
    public float attackRange = 3f;

    public CombatAnimationData[] combatAnimations;
    public float attackCooldownMin = 6f;
    public float attackCooldownMax = 10f;
    public float attackCooldownTimer = 0f;
    public bool isAttacking = false;

    [Header("Searching")]
    public float searchTimer = 0f;
    public float maxSearchTime = 15f;
    public Vector3 lastKnownPlayerPosition;

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public Rigidbody rb;
    public GameObject player;
    protected Collider playerDetectionCollider;

    [Header("Vision")]
    public float visionRange = 10f;
    public float visionAngle = 90f; // 90� means 45� left & right
    public LayerMask visionMask;    // Only raycast against Player + environment

    [Header("Effects")]
    public ParticleSystem takeDamageEffect;

    [Header("Death Logic")]
    public bool isDead = false;
    public float despawnTimer = 10f;
    public float removeColliderTimer = 3f;
    private bool collidersRemoved = false;
    public LayerMask groundLayerMask;
    private bool fadeStarted = false;
    public float fadeDuration = 2f;
    private Vector3 originalScale;
    [HideInInspector] public GameObject poolPrefabReference;

    [Header("Loot Drop Settings")]
    public GameObject lootBagPrefab; // assign your loot bag prefab here
    public List<PossibleLoot> possibleLoot = new(); // filled per enemy in Inspector

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player");
        playerDetectionCollider = player.transform.Find("PlayerDetection").GetComponent<Collider>();
        visionMask = LayerMask.GetMask("Default", "PlayerDetection");
        originalScale = transform.localScale;
        lootBagPrefab = Resources.Load<GameObject>("3D/InteractableProps/Lootbag");
    }

    #region State Machine
    protected virtual void Update()
    {
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            SetState(EnemyState.Dead);
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Wandering:
                UpdateWander();
                break;
            case EnemyState.Combat:
                UpdateCombat();
                break;
            case EnemyState.Searching:
                UpdateSearching();
                break;
            case EnemyState.Dead:
                UpdateDead();
                break;
        }
    }

    protected void SetState(EnemyState newState)
    {
        if (currentState == newState) return;
        if (!CanChangeState(newState)) return;

        // Exit old state
        switch (currentState)
        {
            case EnemyState.Idle: EndIdle(); break;
            case EnemyState.Wandering: EndWander(); break;
            case EnemyState.Combat: EndCombat(); break;
            case EnemyState.Searching: EndSearching(); break;
        }

        currentState = newState;

        // Enter new state
        switch (newState)
        {
            case EnemyState.Idle: EnterIdle(); break;
            case EnemyState.Wandering: EnterWander(); break;
            case EnemyState.Combat: EnterCombat(); break;
            case EnemyState.Searching: EnterSearching(); break;
            case EnemyState.Dead: EnterDead(); break;
        }
    }

    public void SetIdleState()
    {
        currentState = EnemyState.Idle;
    }


    protected virtual bool CanChangeState(EnemyState newState)
    {
        if (isAttacking && newState != EnemyState.Dead)
            return false;
        return true;
    }

    #endregion

    #region Idle

    protected virtual void EnterIdle()
    {
        idleTimer = Random.Range(minIdleCooldown, maxIdleCooldown);
        agent.ResetPath();             // Stop moving
        PlayBool("Idle", true);
    }

    protected virtual void UpdateIdle()
    {
        // Vision detection
        if (DetectPlayer()) return;

        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            SetState(EnemyState.Wandering);
        }
    }

    protected virtual void EndIdle()
    {
        PlayBool("Idle", false);
    }

    #endregion

    #region Combat

    protected virtual void EnterCombat()
    {
        angry = true;
    }

    protected virtual void UpdateCombat()
    {
        if (isAttacking) return; // Attacking: wait for animation event to end
        attackCooldownTimer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        // If player is out of attack range  chase them
        if (distance > attackRange)
        {
            agent.stoppingDistance = attackRange;
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
            PlayBool("Running", true);
            PlayBool("Idle", false);
            FacePlayer();
            return;
        }

        // In range  stop moving & attack if cooldown ready
        agent.isStopped = true;
        PlayBool("Running", false);
        PlayBool("Idle", true);
        agent.ResetPath();
        FacePlayer();

        if (attackCooldownTimer <= 0f)
        {
            StartAttack();
        }
    }

    protected virtual void StartAttack()
    {
        if (combatAnimations == null || combatAnimations.Length == 0) return;

        // Pick a random attack
        var choice = combatAnimations[Random.Range(0, combatAnimations.Length)];

        isAttacking = true;

        // Freeze rotation if that attack requires it
        if (choice.freezeRotation)
            canRotate = false;

        PlayTrigger(choice.animation);

        // Reset cooldown
        attackCooldownTimer = Random.Range(attackCooldownMin, attackCooldownMax);
    }

    protected virtual void ChooseAttack(int chosenInt)
    {
        if (combatAnimations == null || combatAnimations.Length == 0) return;

        // Pick a random attack
        var choice = combatAnimations[chosenInt];

        isAttacking = true;

        // Freeze rotation if that attack requires it
        if (choice.freezeRotation)
            canRotate = false;

        PlayTrigger(choice.animation);

        // Reset cooldown
        attackCooldownTimer = Random.Range(attackCooldownMin, attackCooldownMax);
    }

    public virtual void EndAttack()
    {
        isAttacking = false;
        canRotate = true;
        agent.isStopped = false;
    }

    protected virtual void EndCombat()
    {
        angry = false;
        PlayBool("Running", false);
        PlayBool("Idle", false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerDetection"))
        {
            playerInRange = true;
            SetState(EnemyState.Combat);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerDetection"))
        {
            playerInRange = false;
            lastKnownPlayerPosition = player.transform.position;
            searchTimer = maxSearchTime;
            SetState(EnemyState.Searching);
        }
    }

    protected void PerformAttack(string combatAnimation)
    {
        PlayTrigger(combatAnimation);
    }

    #endregion

    #region Searching

    protected virtual void EnterSearching()
    {
        agent.stoppingDistance = 0.5f;
        agent.SetDestination(lastKnownPlayerPosition);
        PlayBool("Running", true);
    }

    protected virtual void UpdateSearching()
    {
        if (DetectPlayer()) return;

        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            SetState(EnemyState.Wandering);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            SearchAroundArea();
        }
    }

    protected virtual void EndSearching()
    {
        agent.ResetPath();
        PlayBool("Running", false);
    }

    protected virtual void SearchAroundArea()
    {
        Vector3 randomPoint = RandomNavSphere(lastKnownPlayerPosition, WanderRadius);
        agent.SetDestination(randomPoint);
    }

    #endregion

    #region Wandering

    protected virtual void EnterWander()
    {
        PlayBool("Walk", true);
        SetNewWanderDestination();
    }

    protected virtual void UpdateWander()
    {
        // Check player vision
        if (DetectPlayer()) return;

        // If reached wander destination -> switch to idle
        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            SetState(EnemyState.Idle);
        }
    }


    protected virtual void EndWander()
    {
        PlayBool("Walk", false);
    }

    protected virtual void SetNewWanderDestination()
    {
        Vector3 destination = wanderCloseToSpawn
            ? RandomNavSphere(spawnPosition, WanderRadius)
            : RandomNavSphere(transform.position, WanderRadius);

        agent.stoppingDistance = 0f;
        agent.SetDestination(destination);
    }

    #endregion

    #region HelperFunctions

    private Vector3 RandomNavSphere(Vector3 origin, float distance)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance + origin;

        return NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, distance, NavMesh.AllAreas)
            ? hit.position
            : origin;
    }

    public virtual void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;
        if (direction.magnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }


    public void FacePlayer()
    {
        if (player != null)
        {
            FaceTarget(player.transform.position);
        }
    }

    protected void PlayBool(string name, bool value)
    {
        if (animator == null) return;
        animator.SetBool(name, value);
    }

    protected void PlayTrigger(string name)
    {
        if (animator == null) return;
        animator.SetTrigger(name);
    }

    protected virtual bool DetectPlayer()
    {
        if (currentState == EnemyState.Dead) return false;

        Vector3 targetPos = playerDetectionCollider.bounds.center;
        Vector3 toPlayer = (targetPos - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPos);

        // Distance check
        if (distance > visionRange) return false;

        // Angle (forward cone)
        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > visionAngle * 0.5f) return false;

        // Line of sight (raycast)
        Ray ray = new Ray(transform.position + Vector3.up, toPlayer);
        if (Physics.Raycast(ray, out RaycastHit hit, visionRange, visionMask))
        {
            if (hit.collider == playerDetectionCollider)
            {
                SetState(EnemyState.Combat);
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Taking Damage and Death Logic

    public virtual void TakeDamage(float damage)
    {
        if (currentState == EnemyState.Dead) return;

        currentHealth -= damage;

        ParticleSystem effect = Instantiate(takeDamageEffect, transform.position, Quaternion.identity);
        Destroy(effect.gameObject, effect.main.duration + 0.5f);

        PlayTrigger("Hit");
    }


    protected virtual void EnterDead()
    {
        agent.ResetPath();
        agent.enabled = false;

        rb.isKinematic = false;
        rb.useGravity = true;

        PlayTrigger("Death");
    }

    protected virtual void UpdateDead()
    {
        despawnTimer -= Time.deltaTime;
        removeColliderTimer -= Time.deltaTime;

        if (!collidersRemoved)
        {
            bool grounded = IsGrounded();
            bool forceRemove = removeColliderTimer <= -2f;

            if (removeColliderTimer <= 0f && (grounded || forceRemove))
            {
                RemoveAllCollidersAndFreezeBody();
                collidersRemoved = true;

                if (!fadeStarted)
                {
                    DropLoot();
                    fadeStarted = true;
                    StartCoroutine(FadeOutAndPlayEffect());
                }
            }
        }

        if (despawnTimer <= 0f)
        {
            EndDead();
        }
    }


    protected virtual void EndDead()
    {
        ResetEnemy();
        Despawn();
    }

    protected virtual void Despawn()
    {
        if (MobPoolManager.Instance && poolPrefabReference)
        {
            MobPoolManager.Instance.ReturnToPool(poolPrefabReference, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void DropLoot()
    {
        if (lootBagPrefab == null || possibleLoot == null || possibleLoot.Count == 0)
            return;

        // Gather rolled items
        List<StoredItem> droppedItems = new();

        foreach (var loot in possibleLoot)
        {
            if (Random.value <= loot.chance)
            {
                droppedItems.Add(new StoredItem
                {
                    itemID = loot.itemID,
                    amount = loot.amount
                });
            }
        }

        // If nothing was rolled, don’t spawn a bag
        if (droppedItems.Count == 0)
            return;

        // Spawn the loot bag at the corpse position
        Vector3 spawnPos = transform.position + Vector3.up * 0.25f;
        Quaternion rot = Quaternion.identity;

        GameObject lootObj = Instantiate(lootBagPrefab, spawnPos, rot);

        // Fill it with rolled items
        StorageObject storage = lootObj.GetComponent<StorageObject>();
        if (storage != null)
        {
            foreach (var item in droppedItems)
                storage.AddStoredItem(item.itemID, item.amount);
        }
    }

    public virtual void ResetEnemy()
    {
        // Reset stats
        currentHealth = maxHealth;
        isDead = false;
        isAttacking = false;

        // Reset timers
        despawnTimer = 10f;
        removeColliderTimer = 3f;
        collidersRemoved = false;

        // Reset physics
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // Re-enable colliders
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = true;

        // Reset NavMeshAgent
        agent.enabled = true;
        agent.isStopped = false;
        agent.ResetPath();

        // Reset position (mobGenerator will place it)
        transform.rotation = Quaternion.identity;

        // Reset animation
        animator.Rebind();     // Reset animator to initial state
        animator.Update(0f);    // Force apply
        transform.localScale = originalScale;
    }

    protected virtual IEnumerator FadeOutAndPlayEffect()
    {

        // 2. Scale fade over time
        Vector3 initialScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.zero; // Ensure final state
    }


    protected virtual bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, 1f, groundLayerMask);
    }

    protected virtual void RemoveAllCollidersAndFreezeBody()
    {
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    #endregion
}
