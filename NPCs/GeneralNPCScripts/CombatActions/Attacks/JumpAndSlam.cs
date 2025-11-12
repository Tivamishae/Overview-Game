using UnityEngine;
using System.Collections;

public class JumpAndSlam : MeleeAttack
{
    [Header("Jump & Slam Settings")]
    [SerializeField] private float jumpForce = 8f;       // vertical force
    [SerializeField] private float airTime = 2f;         // total time before slam
    [SerializeField] private float slamDamage = 20f;     // damage dealt on impact
    [SerializeField] private float slamRadius = 7f;      // radius of AoE
    [SerializeField] private float slamDuration = 0.35f; // hitbox duration
    [SerializeField] private string slamVFXPath = "3D/Enemies/GroundEnemies/BellGuardian/Impact";
    [SerializeField] private AudioClip slamClip;

    public override string AnimationTrigger => "JumpAndSlam";

    private bool inAir;

    protected override IEnumerator PerformAttackRoutine(NPC npc)
    {
        // --- Phase 1: Chase until in range ---
        while (Vector3.Distance(npc.transform.position, npc.player.transform.position) > range)
        {
            AngerReaction?.Invoke(npc);
            yield return null;
        }

        // --- Phase 2: Stop & wind-up ---
        npc.StopMoving();
        npc.FacePlayer();
        npc.PlayTrigger(AnimationTrigger);
        if (preAttackDelay > 0f)
            yield return new WaitForSeconds(preAttackDelay);

        // --- Phase 3: Jump ---
        Jump(npc);

        // Wait for air time
        yield return new WaitForSeconds(airTime);

        // --- Phase 4: Slam ---
        Slam(npc);

        // cooldown phase
        yield return new WaitForSeconds(hitboxDuration);
    }

    private void Jump(NPC npc)
    {
        inAir = true;

        npc.agent.enabled = false;

        npc.rb.isKinematic = false;
        npc.rb.useGravity = true;

        npc.FreezeAllButYRotation(npc.rb);

        npc.rb.linearVelocity = new Vector3(0, jumpForce, 0);
    }

    private void Slam(NPC npc)
    {
        npc.rb.constraints = RigidbodyConstraints.None;
        npc.rb.isKinematic = true;
        npc.rb.useGravity = false;
        npc.agent.enabled = true;
        npc.agent.Warp(npc.transform.position);
        inAir = false;

        // Audio
        if (slamClip)
            AudioSystem.Instance.PlayClipFollow(slamClip, npc.transform, 1f);

        // VFX
        GameObject impactPrefab = Resources.Load<GameObject>(slamVFXPath);
        if (impactPrefab != null)
        {
            Vector3 groundPos = npc.transform.position;
            GameObject impact = Object.Instantiate(impactPrefab, groundPos, Quaternion.identity);
            Object.Destroy(impact, 3f);
        }

        // Create expanding AoE hitbox
        GameObject hitbox = new GameObject("SlamHitbox");
        hitbox.transform.SetParent(npc.transform);
        hitbox.transform.localPosition = Vector3.zero;

        var h = hitbox.AddComponent<UniversalHitCollider>();
        h.Initialize(
            npc.transform,
            UniversalHitCollider.HitboxType.Expanding,
            slamDamage,
            slamRadius,
            slamDuration
        );
    }
}
