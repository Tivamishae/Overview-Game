using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class NPC : MonoBehaviour
{
    #region Fields

    [Header("General NPC Settings")]
    public string npcName;
    public float baseSpeed;
    public float runningMultiplier;
    public bool angry;
    public float maxHealth = 100;
    public float currentHealth;

    [Header("State Handling")]
    public NPCState currentState;
    public enum NPCState { Idle, Angry, Dead };
    public IIdleReaction idleReaction;
    public Vector3 spawnPosition;
    public bool wanderCloseToSpawn;

    [Header("Anger")]
    public float angerDuration;
    public float angerTimer;
    public Attack[] attacks;
    public bool isAttacking = false;
    private IAngerTrigger[] _angerTriggers;
    public bool wasDamaged = false;

    [Header("References")]
    public NavMeshAgent agent;
    protected Animator animator;
    public Rigidbody rb;
    public GameObject player;


    [Header("Death")]
    public float despawnTimer = 10f;
    public float despawnDuration = 10f;
    public LayerMask groundLayerMask;
    public float removeColliderTimer = 3f;
    public bool collidersRemoved = false;
    public bool fadeStarted;
    public float fadeDuration = 2f;
    protected Vector3 originalScale;
    [HideInInspector] public GameObject poolPrefabReference;
    public bool destroyOnDeath;

    [Header("Loot")]
    public GameObject lootBagPrefab;
    public List<PossibleLoot> possibleLoot = new();

    [Header("Audio")]
    protected AudioClip takeDamageSound;
    public string takeDamageSoundPath;
    protected AudioClip getHurtSound;
    public string getHurtSoundPath;
    protected AudioClip deathSound;
    public string deathSoundPath;
    protected AudioClip idleSound;
    public string idleSoundPath;
    protected AudioClip angerSound;
    public string angerSoundPath;

    [Header("Visual")]
    public ParticleSystem takeDamageEffect;
    public GameObject npcCanvas;
    private Resourcebar healthBar;

    [Header("Detection")]
    public bool canSeePlayer = false;
    public bool canHearPlayer = false;
    public bool noticesPlayer = false;
    public bool playerInHearingRange = false;
    public float visionRange = 10f;
    public float visionAngle = 90f;
    public LayerMask visionMask;
    protected Collider playerDetectionCollider;


    #endregion

    #region Start

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        idleReaction = GetComponent<IIdleReaction>();
        _angerTriggers = GetComponents<IAngerTrigger>();
        Debug.Log($"{name} found {_angerTriggers.Length} anger triggers.");
        attacks = GetComponents<Attack>();
        lootBagPrefab = Resources.Load<GameObject>("3D/InteractableProps/Lootbag");
        CreateNPCHealthAndName();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerDetectionCollider = player.transform.Find("PlayerDetection")?.GetComponent<Collider>();

        originalScale = transform.localScale;
    }

    #endregion

    #region Continous

    protected virtual void Update()
    {
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            SetState(NPCState.Dead);
        }

        if (currentState != NPCState.Dead)
        {
            DetectPlayer();
        }

        if (currentState == NPCState.Idle)
            EvaluateAngerTriggers();

        if (attacks.Length > 0 && attacks != null)
        {
            UpdateAttackCooldowns();
        }

        HandleState();
        wasDamaged = false;
    }

    private void EvaluateAngerTriggers()
    {
        if (_angerTriggers == null || _angerTriggers.Length == 0)
            return;

        foreach (var trigger in _angerTriggers)
        {
            if (trigger != null && trigger.ShouldTrigger(this))
            {
                SetState(NPCState.Angry);
                break;
            }
        }
    }

    public void UpdateAttackCooldowns()
    {
        int counter = 0;
        Attack shortestCooldown = attacks[0];
        foreach (Attack attack in attacks)
        {
            if (attack.cooldownTimer > 0)
            {
                attack.cooldownTimer -= Time.deltaTime;
                counter += 1;
                if (shortestCooldown.cooldownTimer > attack.cooldownTimer)
                {
                    shortestCooldown = attack;
                }
            }
        }
        if (counter == attacks.Length)
        {
            shortestCooldown.AngerReaction?.Invoke(this);
        }
    }

    #endregion

    #region State Machine

    protected void HandleState()
    {
        switch (currentState)
        {
            case NPCState.Idle:
                UpdateIdle();
                break;
            case NPCState.Angry:
                UpdateAngry();
                break;
            case NPCState.Dead:
                UpdateDead();
                break;
        }
    }

    protected void SetState(NPCState newState)
    {
        if (currentState == newState) return;
        if (!CanChangeState(newState)) return;

        // Exit old state
        switch (currentState)
        {
            case NPCState.Idle: EndIdle(); break;
            case NPCState.Angry: EndAngry(); break;
        }

        currentState = newState;

        // Enter new state
        switch (newState)
        {
            case NPCState.Idle: EnterIdle(); break;
            case NPCState.Angry: EnterAngry(); break;
            case NPCState.Dead: EnterDead(); break;
        }
    }
    protected virtual bool CanChangeState(NPCState newState)
    {
        return true;
    }

    public void SetIdleState()
    {
        currentState = NPCState.Idle;
    }

    #endregion

    #region Helper Functions

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

    public virtual void MoveTowards(Vector3 targetPos)
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.SetDestination(targetPos);
        }
        else
        {
            // fallback: simple transform movement
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                Time.deltaTime * baseSpeed
            );
        }

        FaceTarget(targetPos);
    }

    public virtual void RunAwayFrom(Vector3 threatPos)
    {
        Vector3 dir = (transform.position - threatPos).normalized;
        Vector3 fleeTarget = transform.position + dir * 10f; // run 10m away (configurable)

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.SetDestination(fleeTarget);
        }
        else
        {
            transform.position += dir * (baseSpeed * runningMultiplier) * Time.deltaTime;
        }

        FaceTarget(fleeTarget);
    }

    public virtual void StopMoving()
    {
        if (agent != null && agent.enabled)
            agent.isStopped = true;
    }

    public virtual void StartMoving()
    {
        if (agent != null && agent.enabled)
            agent.isStopped = false;
    }

    public void PlayBool(string name, bool value)
    {
        if (animator == null) return;
        animator.SetBool(name, value);
    }

    public void PlayTrigger(string name)
    {
        if (animator == null) return;
        animator.SetTrigger(name);
    }

    public void PlaySound(AudioClip clip)
    {
        AudioSystem.Instance.PlayClipFollow(clip, transform, 1f);
    }

    public virtual void SetNewWanderDestination(float wanderRadius)
    {
        Vector3 destination = wanderCloseToSpawn
            ? RandomNavSphere(spawnPosition, wanderRadius)
            : RandomNavSphere(transform.position, wanderRadius);

        agent.stoppingDistance = 0f;
        agent.SetDestination(destination);
    }

    private Vector3 RandomNavSphere(Vector3 origin, float distance)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance + origin;

        return UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out UnityEngine.AI.NavMeshHit hit, distance, UnityEngine.AI.NavMesh.AllAreas)
            ? hit.position
            : origin;
    }

    #endregion

    #region Idle

    protected virtual void EnterIdle()
    {
        StartMoving();
        PlayBool("Running", false);
    }

    protected virtual void UpdateIdle()
    {
        idleReaction?.ExecuteIdle(this);
    }

    protected virtual void EndIdle()
    {
        agent.ResetPath();
    }

    #endregion

    #region Angry

    protected virtual void EnterAngry()
    {
        angerTimer = angerDuration;
        PlayBool("Walking", false);

        PlaySound(angerSound);
    }

    protected virtual void UpdateAngry()
    {
        if (!noticesPlayer)
        {
            angerTimer -= Time.deltaTime;
        }
        else
        {
            angerTimer = angerDuration;
        }

        if (angerTimer <= 0f || player == null)
        {
            SetState(NPCState.Idle);
            return;
        }

        if (attacks != null && attacks.Length > 0)
        {
            HandleAttacks();
        }
        else
        {
            AngerReactions.Instance.RunAwayReaction(this);
        }
    }


    protected virtual void EndAngry()
    {
        EndAllAttacks();
        PlayBool("Running", false);
    }

    #endregion

    #region Combat

    public void takeDamage(float damage)
    {
        currentHealth -= damage;
        wasDamaged = true;
    }

    protected virtual void HandleAttacks()
    {
        if (isAttacking)
            return;

        if (attacks == null || attacks.Length == 0)
        {
            AngerReactions.Instance.RunAwayReaction(this);
            return;
        }

        Attack attack = attacks[Random.Range(0, attacks.Length)];
        if (attack != null && attack.cooldownTimer <= 0)
        {
            isAttacking = true;
            attack.Execute(this);
        }
    }


    public void EndAttack()
    {
        isAttacking = false;
    }

    public void EndAllAttacks()
    {
        foreach (Attack attack in attacks)
        {
            attack.StopPerformAttack();
        }
        isAttacking = false;
    }

    #endregion

    #region Detection

    protected virtual void DetectPlayer()
    {
        bool canSeePlayer = Vision();
        bool canHearPlayer = playerInHearingRange;

        noticesPlayer = canSeePlayer || canHearPlayer;
    }
    protected virtual bool Vision()
    {
        if (currentState == NPCState.Dead || player == null)
            return false;

        Vector3 targetPos = playerDetectionCollider.bounds.center;
        Vector3 eyePos = transform.position + Vector3.up * 1.6f;
        Vector3 toPlayer = (targetPos - eyePos).normalized;
        float distance = Vector3.Distance(eyePos, targetPos);

        if (distance > visionRange)
            return false;

        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > visionAngle * 0.5f)
            return false;

        if (Physics.Raycast(eyePos, toPlayer, out RaycastHit hit, visionRange, visionMask))
        {
            if (hit.collider == playerDetectionCollider)
                return true;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerDetection"))
            playerInHearingRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerDetection"))
            playerInHearingRange = false;
    }

    #endregion

    #region Dead State

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
        ResetNPC();
        Despawn();
    }

    #endregion

    #region Death Helper Functions

    public virtual void Despawn()
    {
        if (destroyOnDeath)
        {
            Destroy(gameObject);
            return;
        }
        if (MobPoolManager.Instance && poolPrefabReference)
        {
            MobPoolManager.Instance.ReturnToPool(poolPrefabReference, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public virtual void ResetNPC()
    {
        // Reset stats
        currentHealth = maxHealth;
        isAttacking = false;
        noticesPlayer = false;

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

        if (droppedItems.Count == 0)
            return;

        Vector3 spawnPos = transform.position + Vector3.up * 0.25f;
        Quaternion rot = Quaternion.identity;

        GameObject lootObj = Instantiate(lootBagPrefab, spawnPos, rot);

        StorageObject storage = lootObj.GetComponent<StorageObject>();
        if (storage != null)
        {
            foreach (var item in droppedItems)
                storage.AddStoredItem(item.itemID, item.amount);
        }
    }

    protected virtual IEnumerator FadeOutAndPlayEffect()
    {

        Vector3 initialScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.zero;
    }


    protected virtual bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, 1f, groundLayerMask);
    }

    public void FreezeAllButYRotation(Rigidbody rb)
    {
        // Freeze position on X and Z so it can only move vertically
        // Freeze rotation on X and Z, but keep Y free (so it can face player if needed)
        rb.constraints = RigidbodyConstraints.FreezePositionX
                       | RigidbodyConstraints.FreezePositionZ
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;
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

    #region Visual

    private void CreateNPCHealthAndName()
    {
        GameObject canvasPrefab = Resources.Load<GameObject>("2D/RuntimeCanvases/NPCHealthAndName");

        if (canvasPrefab != null)
        {
            npcCanvas = Instantiate(canvasPrefab, transform);
            npcCanvas.transform.localPosition = new Vector3(0, 1f, 0); // Adjust Y as needed
            npcCanvas.AddComponent<FaceCamera>(); // Optional: makes it face the player camera
            healthBar = npcCanvas.transform.Find("HealthBarContainer").GetComponent<Resourcebar>();
            healthBar.SetMaxResource(maxHealth);
            healthBar.SetResource(maxHealth);

            npcCanvas.transform.Find("NPCName").GetComponent<TextMeshProUGUI>().text = npcName;
        }
        else
        {
            Debug.LogWarning("NPCHealthAndName prefab not found in Resources/2D/RuntimeCanvases/");
        }
    }

    #endregion

}