using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization;

public interface ICombatAction
{
    float Cooldown { get; }
    string AnimationTrigger { get; }
    void Execute(NPC npc);
}

public abstract class Movement : MonoBehaviour, ICombatAction
{
    [Header("General Movement Settings")]
    protected Vector3 location;
    [SerializeField] protected float cooldown;

    public virtual string AnimationTrigger => "Movement";
    public virtual float Cooldown => cooldown;
    private Coroutine activeRoutine;

    public virtual void Execute(NPC npc)
    {
        activeRoutine = StartCoroutine(PerformMovementRoutine(npc));
    }

    public virtual IEnumerator PerformMovementRoutine(NPC npc)
    {

        yield return null;
    }

    public virtual bool ChooseLocation(NPC npc, out Vector3 position)
    {
        position = npc.transform.position;
        return true;
    }

    public virtual void StopMovement()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
    }

    public virtual void StopPerformMovement()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
    }
}

public abstract class Attack : MonoBehaviour, ICombatAction
{
    [Header("General Attack Settings")]
    [SerializeField] protected float range = 2f;
    [SerializeField] protected float cooldown = 5f;
    public float cooldownTimer = 0f;
    [SerializeField] protected int damage = 2;
    [SerializeField] protected float hitboxRadius = 1.2f;
    [SerializeField] protected float hitboxDuration = 0.5f;
    [SerializeField] protected float preAttackDelay = 0.2f; // optional wind-up
    private Coroutine activeRoutine;

    [Header("References")]
    [SerializeField] protected Transform hitBoxLocation;
    [SerializeField] protected GameObject hitboxPrefab;

    public virtual float Cooldown => cooldown;
    public virtual string AnimationTrigger => "Attack";
    public virtual Action<NPC> AngerReaction { get; set; }

    public virtual void Execute(NPC npc)
    {
        StopPerformAttack();
        activeRoutine = StartCoroutine(PerformAttackRoutine(npc));
        cooldownTimer = cooldown;
    }

    protected virtual IEnumerator PerformAttackRoutine(NPC npc)
    {

        // --- Phase 1: Chase until in range ---
        while (Vector3.Distance(npc.transform.position, npc.player.transform.position) > range)
        {
            AngerReaction?.Invoke(npc);
            yield return null;
        }

        // --- Phase 2: Stop, face target, wind-up ---
        npc.StopMoving();
        npc.FacePlayer();
        npc.PlayTrigger(AnimationTrigger);

        if (preAttackDelay > 0f)
            yield return new WaitForSeconds(preAttackDelay);

        // --- Phase 3: Perform the actual attack (override in child if needed) ---
        PerformAttack(npc);

        // --- Phase 4: Wait for hitbox duration before allowing next attack ---
        yield return new WaitForSeconds(hitboxDuration);
    }

    /// <summary>
    /// Override this for custom attack effects (projectiles, AoE, etc.)
    /// </summary>
    protected virtual void PerformAttack(NPC npc)
    {
        CreateHitbox();
    }

    protected void CreateHitbox()
    {
        GameObject hitObj = hitboxPrefab != null
            ? Instantiate(hitboxPrefab, hitBoxLocation.position, hitBoxLocation.rotation)
            : new GameObject("Hitbox");

        hitObj.transform.SetParent(hitBoxLocation, false);

        var hit = hitObj.AddComponent<UniversalHitCollider>();
        hit.Initialize(
            hitBoxLocation,
            UniversalHitCollider.HitboxType.Static,
            damage,
            hitboxRadius,
            hitboxDuration
        );
    }

    public virtual void StopPerformAttack()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
    }
}
