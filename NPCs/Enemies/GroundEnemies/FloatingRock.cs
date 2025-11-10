/* using UnityEngine;
using UnityEngine.AI;

public class FloatingRock : Enemy
{
    [Header("Projectile Settings")]
    public GameObject fireFireProjectile;
    public GameObject fireIceProjectile;
    public GameObject fire3Projectile;
    public float damage = 10f;
    public Transform firePoint; // spawn point

    [Header("Audio")]
    private AudioClip castSpellClip;
    private AudioClip fireballClip;
    private AudioClip fireballImpactClip;

    [Header("Attack Settings")]
    private int lastAttackIndex = -1;
    private bool freezeThisAttack = true; // set per attack from tuple

    protected void Start()
    {
        castSpellClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/FloatingRock/CastSpell");
        fireballClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/FloatingRock/Fireball");
        fireballImpactClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/FloatingRock/FireballImpact");
    }

    #region Death

    public override void ResetEnemy()
    {
        base.ResetEnemy();

        // FloatingRock specific cleanup (optional)
        lastAttackIndex = -1;
        isAttacking = false;
        freezeThisAttack = false;
        attackCooldownTimer = 0f;
    }

    #endregion

    #region Combat

    protected override void UpdateCombat()
    {
        // never move in combat
        agent.isStopped = true;
        agent.ResetPath();

        // rotate to face the player unless this attack freezes rotation
        if (!isAttacking || !freezeThisAttack)
            FacePlayer();

        // only the attack animation/event controls end; we just trigger when cooldown ready
        if (!isAttacking)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0f)
                StartAttack(); // overridden below to avoid repeating same attack
        }
    }

    protected override void EnterCombat()
    {
        base.EnterCombat();

        agent.isStopped = true;
        agent.ResetPath();

        PlayBool("Walking", false);
    }

    #endregion

    #region Attack

    protected override void StartAttack()
    {
        if (combatAnimations == null || combatAnimations.Length == 0) return;

        int idx;
        if (combatAnimations.Length == 1)
        {
            idx = 0;
        }
        else
        {
            // avoid repeating previous attack index
            do { idx = Random.Range(0, combatAnimations.Length); }
            while (idx == lastAttackIndex);
        }

        lastAttackIndex = idx;
        var choice = combatAnimations[idx];

        isAttacking = true;
        freezeThisAttack = choice.freezeRotation;

        AudioSystem.Instance.PlayClipFollow(castSpellClip, transform, 0.8f);

        PlayTrigger(choice.animation);

        attackCooldownTimer = Random.Range(attackCooldownMin, attackCooldownMax);
    }

    public override void EndAttack()
    {
        isAttacking = false;
        freezeThisAttack = false;

        // Stay turret-still in combat
        agent.isStopped = true;
    }

    #endregion

    #region Projectile

    public void Fire1Attack() => SpawnProjectile(fireFireProjectile, 20f, damage);
    public void Fire2Attack() => SpawnProjectile(fireIceProjectile, 25f, damage);
    public void Fire3Attack() => SpawnProjectile(fire3Projectile, 30f, damage);

    private void SpawnProjectile(GameObject prefab, float speed, float dmg)
    {
        if (!prefab || player == null) return;

        Transform spawn = firePoint ? firePoint : transform;
        Vector3 dir = (player.transform.position + Vector3.up * 1.5f - spawn.position).normalized;

        // Many VFX meshes face +Z; rotate if your prefab expects it
        Quaternion rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90f, 0f, 0f);

        GameObject projectile = Instantiate(prefab, spawn.position, rotation);

        var proj = projectile.GetComponent<EnemyProjectile>();
        var rb = projectile.GetComponent<Rigidbody>();

        proj.damage = dmg;
        proj.rotationOffset = Quaternion.Euler(90f, 0f, 0f);
        proj.impactSound = fireballImpactClip;

        rb.linearVelocity = dir * speed;

        AudioSystem.Instance.PlayClipFollow(fireballClip, projectile.transform, 0.9f);
    }

    #endregion
}
*/