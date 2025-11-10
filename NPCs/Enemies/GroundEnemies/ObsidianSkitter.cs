/* using UnityEngine;
using System.Collections;

public class ObsidianSkitter : Enemy
{
    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public float jumpForwardMultiplier = 1.5f;
    public float attackWindupDelay = 0.1f;

    [Header("Explosion Settings")]
    public ParticleSystem explosionEffect;
    public float explosionDamage = 25f;
    public float explosionRadius = 3f;
    public float explosionDuration = 0.35f;
    public float selfDestructDelay = 3f;
    private bool hasLaunched = false;
    private bool hasExploded = false;
    private float explosionDelay = 0.5f;
    private int stepsUntilGrunt = 0;

    [Header("Audio")]
    private AudioClip explosionClip;
    private AudioClip stepClip;
    private AudioClip gruntClip;
    private AudioClip deathClip;

    private void Start()
    {
        explosionClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/ObsidianSkitter/Explosion");
        stepClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/ObsidianSkitter/Step");
        gruntClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/ObsidianSkitter/Grunt");
        deathClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/ObsidianSkitter/Death");
    }

    #region Combat

    protected override void UpdateCombat()
    {
        // If already exploding or dead, do nothing
        if (hasExploded || currentHealth <= 0) return;

        // If in mid-jump, skip base combat logic
        if (hasLaunched)
        {
            explosionDelay -= Time.deltaTime;
            return;
        }

        PlayBool("Running", true);

        float distance = Vector3.Distance(transform.position, player.transform.position);

        // Chase the player until within attack range
        if (distance > attackRange)
        {
            agent.stoppingDistance = attackRange;
            agent.SetDestination(player.transform.position);
            FacePlayer();
        }
        else
        {
            agent.ResetPath();

            PlayTrigger("Attack");
            PlayBool("isJumping", true);
        }
    }

    private void Jump()
    {
        hasLaunched = true;

        // Disable NavMesh and enable physics jump
        agent.enabled = false;

        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            Vector3 dir = (player.transform.position - transform.position).normalized;
            float forwardForce = jumpForwardMultiplier * jumpForce;
            Vector3 jumpVector = (dir * forwardForce) + (Vector3.up * jumpForce);
            rb.AddForce(jumpVector, ForceMode.VelocityChange);
        }

        // If no collision happens within X seconds  die normally
        StartCoroutine(SelfDestructTimer());
    }
    private IEnumerator SelfDestructTimer()
    {
        yield return new WaitForSeconds(selfDestructDelay);

        if (!hasExploded && this != null)
            DieExplode();
    }

    // Detect collision to trigger explosion
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasLaunched || hasExploded || explosionDelay > 0) return;

        hasExploded = true;
        DieExplode();
    }

    // ---------- EXPLOSION ----------
    private void DieExplode()
    {
        // Spawn VFX
        if (explosionEffect)
        {
            ParticleSystem fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            fx.Play();
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
        }

        // Spawn AoE damage (not parented)
        GameObject hitboxObj = new GameObject("SkitterExplosionHitbox");
        hitboxObj.transform.position = transform.position;

        var hitbox = hitboxObj.AddComponent<UniversalHitCollider>();
        hitbox.Initialize(
            null,                                      // no parent
            UniversalHitCollider.HitboxType.Expanding,
            explosionDamage,
            explosionRadius,
            explosionDuration
        );

        AudioSystem.Instance.PlayClipAtPoint(explosionClip, transform.position, 1f);

        // TODO: Return to pool (for now just destroy)
        Destroy(gameObject, 0.05f);
    }

    #endregion

    #region Death

    // ---------- NORMAL DEATH (PLAYER KILLS SKITTER BEFORE JUMP) ----------
    public override void TakeDamage(float amount)
    {
        if (hasExploded) return; // ignore damage if already exploding

        base.TakeDamage(amount);

        if (currentHealth <= 0 && !hasExploded)
        {
            // Let base Enemy handle normal death (animation + fade)
            // Do NOT explode on hit damage
        }
    }

    private void DieNormal()
    {
        if (hasExploded) return;
        currentHealth = 0;
        SetState(EnemyState.Dead); // uses base death logic
    }

    // ---------- RESET FOR POOLING ----------
    public override void ResetEnemy()
    {
        base.ResetEnemy();

        hasLaunched = false;
        hasExploded = false;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    #endregion

    #region Audio

    private void Step()
    {
        AudioSystem.Instance.PlayClipFollow(stepClip, transform, 0.5f);
        stepsUntilGrunt--;
        if (stepsUntilGrunt == 0)
        {
            AudioSystem.Instance.PlayClipFollow(gruntClip, transform, 1f);
            stepsUntilGrunt = Random.Range(8, 16);
        }
    }

    

    #endregion
}
*/