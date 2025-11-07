using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(HumanMover))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class InteractableNPC : MonoBehaviour
{
    [Header("Movement Settings")]
    public float wanderRadius = 10f;
    public float idleTime = 3f;

    [Header("Anger Settings")]
    public float attackDistance = 2f;
    public float fleeDistance = 10f;
    public bool isBossSummon = false;
    public float sightRange = 30f;
    public float fieldOfView = 120f;
    public float angerDuration = 15f;   // how long to stay angry without seeing player
    private float angerTimer = 0f;
    private Vector3 calmReturnPosition;
    private float pathUpdateRate = 0.25f; // update 4 times per second
    private float pathUpdateTimer = 0f;

    [Header("Speed Settings")]
    protected float baseSpeed = 2f;
    protected float runSpeed = 8f;

    [Header("Behavior Toggles")]
    public bool canWander = false;
    public bool canChase = false;

    private float idleTimer = 0f;
    private Vector3 targetDestination;
    private bool isIdle = false;
    public bool angry = false;

    public string npcName = "NPC";

    public bool isDead = false;

    [Header("Clothes")]
    public bool hasSetGender = false;
    public bool isMale = true;
    public bool needClothes = true;
    public List<GameObject> UpperBodyClothes = new();
    public List<GameObject> LowerBodyClothes = new();
    public List<GameObject> Hats = new();
    public List<GameObject> Hairs = new();
    public GameObject UpperAndLowerContainer;
    public GameObject HatContainer;
    public GameObject HairContainer;

    public HumanMover humanMover;
    public Animator animator;
    public NavMeshAgent agent;
    public GameObject player;

    public GameObject npcCanvas;
    public Resourcebar healthBar;

    public bool isBeingInteractedWith;

    public float Health = 100f;

    public string HitSound;

    public List<StoredItem> loot = new();
    public List<PossibleLoot> possibleLoot = new();

    protected virtual void Awake()
    {
        humanMover = GetComponent<HumanMover>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player");
        RandomizeLoot();
        CreateNPCHealthAndName();

        if (hasSetGender)
        {
            isMale = Random.value < 0.5f;
        }

        if (needClothes)
        {
            LoadClothes();
            ClotheNPC();
        }
    }

    protected virtual void Update()
    {
        if (isDead) return;

        if (Health <= 0)
        {
            Die();
        }

        if (angry)
        {
            ChasePlayer();
        }

        if (canWander)
        {
            Wander();
        }

        HandleAnger();
    }

    #region Clothing Logic
    void ClotheNPC()
    {
        // Upper
        if (UpperBodyClothes.Count > 0)
        {
            var prefab = UpperBodyClothes[Random.Range(0, UpperBodyClothes.Count)];
            AttachClothing(prefab, UpperAndLowerContainer.transform);
        }

        // Lower
        if (LowerBodyClothes.Count > 0)
        {
            var prefab = LowerBodyClothes[Random.Range(0, LowerBodyClothes.Count)];
            AttachClothing(prefab, UpperAndLowerContainer.transform);
        }

        // Hat
        if (Hats.Count > 0)
        {
            var prefab = Hats[Random.Range(0, Hats.Count)];
            AttachClothing(prefab, HatContainer.transform);
        }

        // Hair
        if (Hairs.Count > 0)
        {
            var prefab = Hairs[Random.Range(0, Hairs.Count)];
            AttachClothing(prefab, HairContainer.transform);
        }
    }

    private void AttachClothing(GameObject prefab, Transform container)
    {
        // Instantiate as a child of the container
        GameObject instance = Instantiate(prefab, container, false);

        // Do NOT reset localPosition, localRotation, or localScale
        // They will remain exactly as defined in the prefab
    }

    private void LoadClothes()
    {
        string genderFolder = isMale ? "Male" : "Female";

        UpperBodyClothes = LoadPrefabs($"3D/NPCAddOns/{genderFolder}/Upper");
        LowerBodyClothes = LoadPrefabs($"3D/NPCAddOns/{genderFolder}/Lower");
        Hats = LoadPrefabs($"3D/NPCAddOns/{genderFolder}/Hats");
        Hairs = LoadPrefabs($"3D/NPCAddOns/{genderFolder}/Hairs");
    }

    private List<GameObject> LoadPrefabs(string path)
    {
        List<GameObject> loaded = new();

        GameObject[] resources = Resources.LoadAll<GameObject>(path);
        foreach (var prefab in resources)
        {
            loaded.Add(prefab);
        }

        return loaded;
    }

    #endregion

    #region Combat Logic
    public virtual void ChasePlayer()
    {
        agent.speed = runSpeed;
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance > attackDistance)
        {
            pathUpdateTimer -= Time.deltaTime;
            if (pathUpdateTimer <= 0f)
            {
                agent.SetDestination(player.transform.position);
                pathUpdateTimer = pathUpdateRate;
            }

            humanMover.SetIsRunning(true);
            humanMover.SetIsMoving(true);
        }
        else
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            humanMover.SetIsMoving(false);
            humanMover.SetIsRunning(false);

            humanMover.TriggerAttack();
        }

        FaceTarget(player.transform.position);
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;

        if (Vector3.Angle(transform.forward, dirToPlayer) < fieldOfView * 0.5f)
        {
            if (Physics.Raycast(transform.position, dirToPlayer, out RaycastHit hit, sightRange))
            {
                return hit.collider.CompareTag("Player");
            }
        }
        return false;
    }

    public void RunFromPlayer()
    {
        if (player == null || agent == null) return;

        agent.speed = runSpeed;
        humanMover.SetIsMoving(true);
        humanMover.SetIsRunning(true);

        Vector3 target;

        // Fallback: run away from player
        Vector3 awayDir = (transform.position - player.transform.position).normalized;
        target = transform.position + awayDir * fleeDistance;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    public virtual void OnPlayerHit(float damage)
    {
        if (isDead) return;

        Health -= damage;
        healthBar.SetResource(Health);

        TriggerAnger();
        humanMover.TriggerTakeDamage();

        AudioSystem.Instance.PlayClipFollow(Resources.Load<AudioClip>("Sounds/NPC/HitSound"), transform, 1f);
        AudioClip hitSound = Resources.Load<AudioClip>("Sounds/InteractableObjects/" + HitSound);
        AudioSystem.Instance.PlayClipAtPoint(hitSound, this.transform.position, 1f);

        StartCoroutine(TemporarilyStopAgent(0.3f));
    }

    public void TriggerAnger()
    {
        if (!angry)
        {
            // Save the position before chasing
            calmReturnPosition = transform.position;

            angry = true;
            angerTimer = angerDuration;
            humanMover.SetIsAngry(true);
            AudioSystem.Instance.PlayClipFollow(Resources.Load<AudioClip>("Sounds/NPC/Angry"), transform, 1f);
        }
    }

    protected virtual void CalmDown()
    {
        angry = false;
        humanMover.SetIsAngry(false);

        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = baseSpeed; // or runSpeed if you want them to run
            agent.SetDestination(calmReturnPosition);
        }

        humanMover.SetIsMoving(true);
        humanMover.SetIsRunning(false); // walking back

        StartCoroutine(ReturnToCalmPointRoutine());
        Debug.Log($"{name} has calmed down and is returning to guard position.");
    }

    private IEnumerator ReturnToCalmPointRoutine()
    {
        // Wait until path is ready
        yield return new WaitUntil(() => agent != null && !agent.pathPending);

        // Wait until agent actually reaches the destination
        while (agent != null && agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        // Stop movement animations
        humanMover.SetIsMoving(false);
        humanMover.SetIsRunning(false);
        agent.ResetPath();
        agent.isStopped = true;

        Debug.Log($"{name} returned to calm point and is idle again.");
    }



    public virtual void HandleAnger()
    {
        if (angry)
        {
            humanMover.SetIsAngry(true);

            if (player == null || !CanSeePlayer())
            {
                // Tick down if player missing or out of sight
                angerTimer -= Time.deltaTime;
            }
            else
            {
                // Reset timer if player visible
                angerTimer = angerDuration;
            }

            if (angerTimer <= 0f)
            {
                CalmDown();
            }
        }
    }


    public void Die()
    {
        isDead = true;
        agent.isStopped = true;
        humanMover.SetIsMoving(false);
        humanMover.enabled = false;

        StartCoroutine(DelayedDeath());
    }

    private IEnumerator DelayedDeath()
    {
        yield return new WaitForSeconds(0.8f);
        
        humanMover.TriggerDeath();
        StorageObject lootStorage = gameObject.AddComponent<StorageObject>();
        lootStorage.storedItems = loot;

        Instantiate(Resources.Load<GameObject>("3D/Effects/LootDropEffect"), transform);

        Destroy(gameObject.GetComponent<InteractableNPC>());

    }

    public virtual void DamageFromAttack() // Called by Animator event
    {
        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist < 4f)
        {
            PlayerStats.Instance.TakeDamage(10f);
        }
    }

    public virtual void ThrowAttack()
    {
    }

    #endregion

    #region Loot Logic
    public void RandomizeLoot()
    {
        loot.Clear(); // Clear any previous loot

        foreach (var item in possibleLoot)
        {
            if (UnityEngine.Random.value <= item.chance)
            {
                int amount = UnityEngine.Random.Range(0, item.amount + 1);

                // Only add if at least 1 item is generated
                if (amount > 0)
                {
                    StoredItem newItem = new StoredItem
                    {
                        itemID = item.itemID,
                        amount = amount
                    };

                    loot.Add(newItem);
                }
            }
        }
    }

    #endregion

    #region Movement Logic
    private IEnumerator TemporarilyStopAgent(float duration)
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(duration);
        agent.isStopped = false;
    }

    protected void Wander()
    {
        agent.speed = baseSpeed;

        // If agent has no destination, assign one immediately
        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
        {
            // Wait idle before setting a new destination
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleTime)
            {
                SetNewDestination();
                idleTimer = 0f;
            }

            humanMover.SetIsMoving(false);
        }
        else
        {
            // Actively walking
            idleTimer = 0f; // Reset idle timer while moving
            humanMover.SetIsMoving(true);
        }
    }


    protected void SetNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            targetDestination = hit.position;
            agent.SetDestination(targetDestination);
        }
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

    #endregion

    #region Interaction Logic
    public void Interact()
    {
        // Play "Talk" sound from Resources/Sounds/NPC/Talk

        if (isDead)
        {
            GetComponent<HumanInteraction>()?.OpenLoot();
        }
        else
        {
            var talkClip = Resources.Load<AudioClip>("Sounds/NPC/Talk");
            AudioSystem.Instance.PlayClipFollow(talkClip, transform, 1f);
            GetComponent<HumanInteraction>()?.Interact();
            animator.SetTrigger("Talk");
        }
    }

    private void CreateNPCHealthAndName()
    {
        GameObject canvasPrefab = Resources.Load<GameObject>("2D/RuntimeCanvases/NPCHealthAndName");

        if (canvasPrefab != null)
        {
            npcCanvas = Instantiate(canvasPrefab, transform);
            npcCanvas.transform.localPosition = new Vector3(0, 1f, 0); // Adjust Y as needed
            npcCanvas.AddComponent<FaceCamera>(); // Optional: makes it face the player camera
            healthBar = npcCanvas.transform.Find("HealthBarContainer").GetComponent<Resourcebar>();
            healthBar.SetMaxResource(Health);
            healthBar.SetResource(Health);

            npcCanvas.transform.Find("NPCName").GetComponent<TextMeshProUGUI>().text = npcName;
        }
        else
        {
            Debug.LogWarning("NPCHealthAndName prefab not found in Resources/2D/RuntimeCanvases/");
        }
    
    }

    public void SetInteractionState(bool state)
    {
        isBeingInteractedWith = state;
    }
    #endregion
}
