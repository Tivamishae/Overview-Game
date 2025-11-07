using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Boss1 : GeneralBossScript
{
    [Header("Teleport Settings")]
    public float teleportInterval = 5f;
    public float teleportRadius = 10f;        // how far around the player to teleport
    public float navmeshSnapDistance = 3f;    // NavMesh.SamplePosition search distance
    public int maxTeleportTries = 8;          // number of tries to find a valid position
    public NavMeshAgent agent;

    [Header("Attack Animations")]
    //private string[] attackTriggers = { "GroundSpikes", "JumpSlam", "SpiritDash", "SpiritScream" };
    public string[] attackTriggers = {"JumpSlam"};

    private float timer;
    private Vector3 savedPlayerPosition;

    //Attacks
    [Header("Jump Slam settings")]
    public float impactRadius = 6f;       // how far the slam reaches
    public float impactDamage = 50f;      // how much damage to deal
    public string impactSound = "Sounds/Bosses/Boss1/Boss1Impact";

    [Header("Spirit Dash Settings")]
    public GameObject shadowPrefab;           // assign in Inspector
    public float bossDashSpeed = 30f;         // boss dash speed
    private Vector3 dashTarget;
    private Vector3 dashStart;
    private Collider bossCollider;
    public string bossDashSound = "Sounds/Bosses/Boss1/BossSpiritDash";

    [Header("Spirit Scream settings")]
    public float spiritExplosionDamage = 50f;
    public float spiritExplosionRadius = 15f;
    public string spiritScreamChargeSound = "Sounds/Bosses/Boss1/SpiritScreamCharge";
    public string spiritScreamExplosionSound = "Sounds/Bosses/Boss1/SpiritScream";

    [Header("Spirit Chase settings")]
    public GameObject groundRipplePrefab;

    [Header("Obelisk Summon Settings")]
    [SerializeField] private List<Vector3> obeliskSpawnPoints = new();
    public string obeliskPath = "3D/Bosses/Boss1/Boss1Spawns/Boss1Obelisk";
    public float riseAmount = 3f;
    public float riseDuration = 1.5f;
    private bool respawnRoutineRunning = false;
    private List<MineableObject> activeObelisks = new();

    [Header("Phase2Settings")]
    public bool phase2 = false;

    private void Start()
    {
        timer = 2f;
        agent = GetComponent<NavMeshAgent>();
        bossCollider = GetComponent<Collider>(); // cache collider
        SummonObelisks();
    }

    private void Update()
    {
        if (player == null || agent == null || health <= 0f || isDead)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            StartCoroutine(DoTeleportCycle());
            timer = teleportInterval;
        }

        if (ShouldFacePlayer)
        {
            FacePlayer();
        }

        for (int i = activeObelisks.Count - 1; i >= 0; i--)
        {
            var obelisk = activeObelisks[i];
            if (obelisk == null) { activeObelisks.RemoveAt(i); continue; }

            if (obelisk.health <= 5f)
            {
                // Sink & destroy
                StartCoroutine(SinkAndDestroy(obelisk.transform, riseAmount, riseDuration));
                activeObelisks.RemoveAt(i);
            }
        }

        // If all obelisks are gone and we’re not already waiting for respawn
        if (activeObelisks.Count == 0 && !respawnRoutineRunning)
        {
            TakeDamage(100);
            StartCoroutine(RespawnObelisksAfterDelay(10f));
        }

        if (health < MaxHealth * 0.5 && phase2 == false)
        {
            triggerPhase2();
        }
    }

    private void triggerPhase2()
    {
        phase2 = true;
    }

    private void SummonObelisks()
    {
        var obeliskPrefab = Resources.Load<GameObject>(obeliskPath);
        if (obeliskPrefab == null) return;

        activeObelisks.Clear();

        foreach (var spawnPos in obeliskSpawnPoints)
        {
            Vector3 undergroundPos = spawnPos - Vector3.up * riseAmount;
            var obeliskObj = Instantiate(obeliskPrefab, undergroundPos, Quaternion.Euler(-90f, 0f, 0f));

            // Set its health to 10
            var mineable = obeliskObj.GetComponent<MineableObject>();
            if (mineable != null)
            {
                mineable.health = 10f;
                activeObelisks.Add(mineable);
            }

            // Animate rise
            StartCoroutine(RiseObelisk(obeliskObj.transform, riseAmount, riseDuration));
        }
    }

    private IEnumerator RiseObelisk(Transform obelisk, float riseAmount, float duration)
    {
        Vector3 startPos = obelisk.position;
        Vector3 endPos = startPos + Vector3.up * riseAmount;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (obelisk == null) yield break;
            obelisk.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (obelisk != null)
            obelisk.position = endPos;
    }

    private IEnumerator SinkAndDestroy(Transform obelisk, float sinkAmount, float duration)
    {
        Vector3 startPos = obelisk.position;
        Vector3 endPos = startPos - Vector3.up * sinkAmount;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (obelisk == null) yield break;
            obelisk.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (obelisk != null)
            Destroy(obelisk.gameObject);
    }

    private IEnumerator RespawnObelisksAfterDelay(float delay)
    {
        respawnRoutineRunning = true;
        yield return new WaitForSeconds(delay);

        SummonObelisks(); // respawn new obelisks
        respawnRoutineRunning = false;
    }

    private System.Collections.IEnumerator DoTeleportCycle()
    {
        // Trigger Teleportation animation
        if (animator != null)
        {
            animator.SetTrigger("Teleportation");
        }

        // Optional: small delay so the animation can start before teleporting
        yield return new WaitForSeconds(0.3f);

        Teleport();

        // Wait a moment after teleport (e.g., for landing effect)
        yield return new WaitForSeconds(0.5f);

        // Trigger one random attack animation
        if (animator != null && attackTriggers.Length > 0)
        {
            string chosenAttack = attackTriggers[Random.Range(0, attackTriggers.Length)];
            animator.SetTrigger(chosenAttack);
        }
    }

    private void Teleport()
    {
        var teleportPrefab = Resources.Load<GameObject>("3D/Bosses/Boss1/Boss1Spawns/TeleportEffect");
        if (teleportPrefab != null)
        {
            var teleportEffect = Instantiate(teleportPrefab, transform.position, Quaternion.identity);
            Destroy(teleportEffect, 5f);
        }
        StartCoroutine(PhaseThroughGround());
    }

    private IEnumerator PhaseThroughGround()
    {
        if (player == null) yield break;

        // Step 1: Find new NavMesh target near player
        Vector3 desiredPos = transform.position;
        bool found = false;

        for (int i = 0; i < maxTeleportTries; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * teleportRadius;
            Vector3 candidate = player.position + offset;

            if (NavMesh.SamplePosition(candidate, out var hit, navmeshSnapDistance, NavMesh.AllAreas))
            {
                desiredPos = hit.position;
                found = true;
                break;
            }
        }

        if (!found) yield break;

        // Step 2: Go underground at current position
        Vector3 undergroundStart = transform.position + Vector3.down * 2f;
        yield return MoveToPoint(undergroundStart, 0.3f);

        // Step 3: Warp underground to new spot
        Vector3 undergroundEnd = desiredPos + Vector3.down * 2f;
        agent.enabled = false; // disable NavMesh control
        transform.position = undergroundEnd;
        yield return new WaitForSeconds(0.2f);

        // Step 4: Move back up to surface at target position
        var teleportPrefab = Resources.Load<GameObject>("3D/Bosses/Boss1/Boss1Spawns/TeleportEffect");
        if (teleportPrefab != null)
        {
            var teleportEffect = Instantiate(teleportPrefab, desiredPos, Quaternion.identity);
            Destroy(teleportEffect, 5f);
        }
        yield return MoveToPoint(desiredPos, 0.3f);
        agent.enabled = true; // re-enable NavMeshAgent
    }

    private IEnumerator MoveToPoint(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
    }

    // Jump slam
    public void JumpUp()
    {

        if (agent != null)
            agent.enabled = false; // disable navmesh control during jump

        Debug.Log($"{name} jumps up! Saving player position {savedPlayerPosition}");

        StartCoroutine(JumpUpCoroutine());
    }

    private IEnumerator JumpUpCoroutine()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * 40f;

        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 40f * Time.deltaTime);
            yield return null;
        }
    }

    // Called in mid-air animation when starting to descend
    public void FallDown()
    {
        Debug.Log($"{name} is falling toward saved position {savedPlayerPosition}");
        StartCoroutine(FallCoroutine());
    }

    private IEnumerator FallCoroutine()
    {
        ShouldFacePlayer = false;
        if (player != null)
            savedPlayerPosition = player.position;
        if (agent != null) agent.enabled = false; // disable agent during fall

        Vector3 target = savedPlayerPosition;
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            target = hit.position;

        // Move downward toward saved position
        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, 60f * Time.deltaTime); // slam speed
            yield return null;
        }

        // Snap to ground safely
        transform.position = target;
        if (agent != null)
        {
            agent.Warp(target);
            agent.enabled = true;
            Impact();
        }
    }

    // Called on landing animation frame

    public void Impact()
    {
        PlaySound(impactSound, 1f, false);
        Debug.Log($"{name} caused an impact at {transform.position}");

        // Main slam effect
        var slamPrefab = Resources.Load<GameObject>("3D/Bosses/Boss1/Boss1Spawns/JumpSlamShock");
        if (slamPrefab != null)
        {
            var slamEffect = Instantiate(slamPrefab, transform.position, Quaternion.identity);
            Destroy(slamEffect, 3f);
        }

        var slamPrefab2 = Resources.Load<GameObject>("3D/Bosses/Boss1/Boss1Spawns/JumpSlamImpact");
        if (slamPrefab2 != null)
        {
            var slamEffect2 = Instantiate(slamPrefab2, transform.position, Quaternion.identity);
            Destroy(slamEffect2, 1f);
        }

        // Damage player if within radius
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= impactRadius)
            {
                Debug.Log($"{name} hit the player with Jump Slam! Dealing {impactDamage} damage.");
                PlayerStats.Instance.TakeDamage(impactDamage);
            }
        }
        StartCoroutine(FacePlayerDelay(0.5f));
    }

    private IEnumerator FacePlayerDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShouldFacePlayer = true;
    }

    //Spirit Dash
    public void SpawnShadow()
    {
        if (player == null || shadowPrefab == null) return;

        // Save starting position
        dashStart = transform.position;

        // Disable collider so boss can pass through player
        if (bossCollider != null)
            bossCollider.enabled = false;

        // Spawn shadow
        Instantiate(shadowPrefab, transform.position, transform.rotation);

        Debug.Log($"{name} spawned SpiritDash shadow at {transform.position}, dashing to {dashTarget}");
    }

    // --------------------
    // Internal helpers
    // --------------------

    private IEnumerator DashToTarget()
    {
        PlaySound(bossDashSound, 1f, false);
        ShouldFacePlayer = false;
        if (agent != null) agent.enabled = false; // disable navmesh during dash

        Vector3 dashDirection = (player.position - dashStart).normalized;
        dashTarget = player.position + dashDirection * 4f;

        while (Vector3.Distance(transform.position, dashTarget) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, dashTarget, bossDashSpeed * Time.deltaTime);
            yield return null;
        }

        if (agent != null)
        {
            agent.Warp(dashTarget);
            agent.enabled = true;
        }

        if (bossCollider != null)
            bossCollider.enabled = true;
        StartCoroutine(FacePlayerDelay(2f));
    }

    // Spirit Scream
    public void SpiritScreamCharge()
    {
        PlaySound(spiritScreamChargeSound, 1f, false);
        var chargePrefab = Resources.Load<GameObject>("3D/Bosses/Boss1/Boss1Spawns/SpiritScreamCharge");
        if (chargePrefab != null)
        {
            var chargeEffect = Instantiate(chargePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Destroy(chargeEffect, 3f);
        }
    }

    // Animation event: called on explosion frame
    public void SpiritScreamExplosion()
    {
        var spiritExplosionPrefab = Resources.Load<GameObject>("3D/Bosses/Boss1/Boss1Spawns/SpiritScreamExplosion");
        PlaySound(spiritScreamExplosionSound, 1f, false);
        if (spiritExplosionPrefab != null)
        {
            var spiritExplosionEffect = Instantiate(spiritExplosionPrefab, transform.position + Vector3.up * 0.5f , Quaternion.identity);
            Destroy(spiritExplosionEffect, 3f);
        }

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= spiritExplosionRadius)
            {
                Debug.Log($"{name} hit the player with Jump Slam! Dealing {spiritExplosionDamage} damage.");
                PlayerStats.Instance.TakeDamage(spiritExplosionDamage);
            }
        }
    }

    //SpiritChase
    private void SpiritChase()
    {
        Instantiate(groundRipplePrefab, this.transform.position, Quaternion.identity);
    }


    private void PlaySound(string path, float volume, bool loop)
    {
        AudioClip clip = AudioPreloader.Instance.GetClip(path);
        if (clip == null)
        {
            Debug.LogError($"GameAmbience: Could not find music at Resources/{path}");
            return;
        }

        Transform followTarget = transform;
        AudioSystem.Instance.PlayClipFollow(clip, followTarget, 1f);
    }


}
