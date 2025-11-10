using UnityEngine;
using System;
using System.Collections;

public interface ICombatAction
{
    float Cooldown { get; }
    Action<NPC> AngerReaction { get; set; }
    string AnimationTrigger { get; }
    void Execute(NPC npc);
}

public abstract class Attack : MonoBehaviour, ICombatAction
{
    [Header("General Attack Settings")]
    [SerializeField] protected float range = 2f;
    [SerializeField] protected float cooldown = 2f;
    [SerializeField] protected int damage = 2;
    [SerializeField] protected float hitboxRadius = 1.2f;
    [SerializeField] protected float hitboxDuration = 0.5f;
    [SerializeField] protected float preAttackDelay = 0.2f; // optional wind-up

    [Header("References")]
    [SerializeField] protected Transform hitBoxLocation;
    [SerializeField] protected GameObject hitboxPrefab;

    public virtual float Cooldown => cooldown;
    public virtual string AnimationTrigger => "Attack";
    public virtual Action<NPC> AngerReaction { get; set; }

    protected float nextAttackTime;

    public virtual void Execute(NPC npc)
    {
        if (Time.time < nextAttackTime)
            return;

        npc.StartCoroutine(PerformAttackRoutine(npc));
    }

    protected virtual IEnumerator PerformAttackRoutine(NPC npc)
    {
        nextAttackTime = Time.time + cooldown;

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

        npc.EndAttack();
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
}
