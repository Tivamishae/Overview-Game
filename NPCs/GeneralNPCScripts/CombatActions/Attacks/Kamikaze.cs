using UnityEngine;
using System.Collections;

public class Kamikaze : Attack
{
    [Header("Kamikaze Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpForwardMultiplier = 1.5f;
    [SerializeField] private float selfDestructDelay = 3f;
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private AudioClip explosionClip;

    private bool hasLaunched;
    private bool hasExploded;
    private Coroutine selfDestructRoutine;

    public override string AnimationTrigger => "Kamikaze"; // Triggered animation name

    protected void Awake()
    {
        // Default chase behavior
        AngerReaction = (npc) => AngerReactions.Instance.ChaseReaction(npc, range);
    }

    protected override IEnumerator PerformAttackRoutine(NPC npc)
    {
        while (Vector3.Distance(npc.transform.position, npc.player.transform.position) > range)
        {
            AngerReaction?.Invoke(npc);
            yield return null;
        }

        // Only trigger once per state
        if (hasLaunched) yield break;

        npc.StopMoving();
        npc.FacePlayer();
        npc.PlayTrigger(AnimationTrigger);

        // --- Jump ---
        Jump(npc);

        // Start fallback self-destruct timer
        selfDestructRoutine = npc.StartCoroutine(SelfDestructTimer(npc));
    }

    private void Jump(NPC npc)
    {
        npc.agent.enabled = false;
        npc.rb.isKinematic = false;
        npc.rb.useGravity = true;

        // Clear any leftover motion
        npc.rb.linearVelocity = Vector3.zero;
        npc.rb.angularVelocity = Vector3.zero;

        // Apply jump + forward impulse
        Vector3 dir = (npc.player.transform.position - npc.transform.position).normalized;
        Vector3 jumpVector = dir * (jumpForwardMultiplier * jumpForce) + Vector3.up * jumpForce;
        npc.rb.AddForce(jumpVector, ForceMode.VelocityChange);
    }

    private IEnumerator SelfDestructTimer(NPC npc)
    {
        yield return new WaitForSeconds(0.1f);
        hasLaunched = true;
        yield return new WaitForSeconds(selfDestructDelay);
        if (!hasExploded && npc != null)
            Explode(npc);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // NOTE: The NPC passes itself into Attack when executed
        // so we must find the parent NPC on collision
        if (hasExploded || !hasLaunched) return;
        NPC npc = GetComponentInParent<NPC>();
        if (npc == null) return;

        Explode(npc);
    }

    private void Explode(NPC npc)
    {
        if (hasExploded) return;
        hasExploded = true;

        // Cancel pending self-destruct
        if (selfDestructRoutine != null)
            npc.StopCoroutine(selfDestructRoutine);

        // Visual + Audio feedback
        if (explosionEffect)
        {
            ParticleSystem fx = Object.Instantiate(explosionEffect, npc.transform.position, Quaternion.identity);
            fx.Play();
            Object.Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
        }

        if (explosionClip)
            AudioSystem.Instance.PlayClipAtPoint(explosionClip, npc.transform.position, 1f);

        // Spawn AoE hitbox
        GameObject hitboxObj = new GameObject("KamikazeExplosionHitbox");
        hitboxObj.transform.position = npc.transform.position;

        var hitbox = hitboxObj.AddComponent<UniversalHitCollider>();
        hitbox.Initialize(
            null,  // No parent, static
            UniversalHitCollider.HitboxType.Expanding,
            damage,
            hitboxRadius,
            hitboxDuration
        );
        npc.Despawn();
    }

    public override void Execute(NPC npc)
    {
        if (hasLaunched) return;
        npc.StartCoroutine(PerformAttackRoutine(npc));
    }

    public override void StopPerformAttack()
    {
        if (selfDestructRoutine != null)
        {
            StopCoroutine(selfDestructRoutine);
            selfDestructRoutine = null;
        }
        base.StopPerformAttack();
    }
}
