using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Bat : FlyingEnemy
{
    [Header("Bat Attack Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private float projectileDamage = 12f;

    [Header("Melee Settings")]
    [SerializeField] private Transform clawOrigin;
    [SerializeField] private float clawDamage = 15f;
    [SerializeField] private float clawRadius = 4f;
    [SerializeField] private float clawDuration = 0.4f;

    [Header("Combat Behavior")]
    [SerializeField] private float stopBeforeMoveDelay = 1.2f;
    [SerializeField] private float minMoveRadius = 6f;
    [SerializeField] private float maxMoveRadius = 10f;
    [SerializeField] private float meleeRange = 6f;
    [SerializeField] private float retreatDistance = 8f;
    [SerializeField] private float projectileCooldown = 2.5f;
    public float isMoving;

    [Header("Audio")]
    private AudioClip projectileClip;
    private AudioClip screechClip;
    private AudioClip wingFlapClip;

    private float nextAttackTime;
    private bool isRelocating;
    private bool isHovering;
    private bool isRetreating;

    protected void Start()
    {
        projectileClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/Bat/ProjectileSound");
        screechClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/Bat/Screech");
        wingFlapClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/Bat/WingFlap");
    }

    protected override void UpdateCombat()
    {
        attackCooldownTimer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        //  If player is too close  melee and retreat
        if (attackCooldownTimer <= 0 && !isAttacking)
        {
            if (distance < meleeRange)
            {
                ChooseAttack(0);
                StartCoroutine(RetreatAfterMelee());
                return;
            }
            else if (distance > meleeRange)
            {
                ChooseAttack(1);
                StartCoroutine(FireThenRelocate());
                return;
            }
        }
        if (!isRetreating && !isRelocating)
        {
            FacePlayer();
        }
    }

    private IEnumerator FireThenRelocate()
    {
        // Hover briefly before moving
        agent.isStopped = true;
        yield return new WaitForSeconds(stopBeforeMoveDelay);

        // Choose a new position around the player
        Vector3 randomDir = Random.insideUnitSphere;
        randomDir.y = 0f;
        randomDir.Normalize();

        Vector3 targetPos = player.transform.position + randomDir * Random.Range(minMoveRadius, maxMoveRadius);
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            isRelocating = true;
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }

        isAttacking = false;

        // Wait until the bat reaches its new position
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= 0.5f);

        // Stop and shoot again
        agent.isStopped = true;
        ChooseAttack(1);

        nextAttackTime = Time.time + projectileCooldown;
        isRelocating = false;
    }

    private IEnumerator RetreatAfterMelee()
    {
        yield return new WaitForSeconds(0.6f); // short delay after melee impact

        // Retreat opposite from player
        Vector3 retreatDir = (transform.position - player.transform.position).normalized;
        Vector3 retreatTarget = transform.position + retreatDir * retreatDistance;

        if (NavMesh.SamplePosition(retreatTarget, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            isRetreating = true;
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }

        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= 0.5f);
        isRetreating = false;
        isAttacking = false;
    }

    // -------------------------------
    //  Animation Event: FireProjectile
    // -------------------------------
    public void FireProjectile()
    {
        AudioSystem.Instance.PlayClipFollow(screechClip, transform, 1f);

        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;
        GameObject obj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        var proj = obj.GetComponent<EnemyProjectile>();
        proj.damage = projectileDamage;

        Vector3 dir = (player.transform.position + Vector3.up * 1.2f - spawnPos).normalized;
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb) rb.linearVelocity = dir * projectileSpeed;

        AudioSystem.Instance.PlayClipFollow(projectileClip, obj.transform, 0.9f);
    }

    // -------------------------------
    //  Animation Event: DealMeleeDamage
    // -------------------------------
    public void DealMeleeDamage()
    {
        GameObject hitObj = new GameObject("BatClawHitbox");
        var hit = hitObj.AddComponent<UniversalHitCollider>();

        hit.Initialize(
            clawOrigin,
            UniversalHitCollider.HitboxType.Static,
            clawDamage,
            clawRadius,
            clawDuration
        );
        AudioSystem.Instance.PlayClipFollow(screechClip, transform, 1f);
    }

    public void WingFlap() => AudioSystem.Instance.PlayClipFollow(wingFlapClip, transform, 1f);
}
