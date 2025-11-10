using UnityEngine;
using UnityEngine.AI;

public class BellGuardian : NPC
{
    [Header("Audio")]
    private AudioClip chargeGruntClip;
    private AudioClip walkGruntClip;
    private AudioClip meleeSwishClip;
    private AudioClip meleeGruntClip;
    private AudioClip stepClip;
    private AudioClip slamClip;
    private AudioClip bellClip;

    [Header("Audio Logic")]
    private int stepsUntilGrunt = 8;
    private int idleUntilGrunt = 10;

    [Header("Attack Settings")]
    public GameObject leftHandBone;
    private bool inAir = false;

    private void Start()
    {
        if (AudioPreloader.Instance)
        {
            chargeGruntClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/BellGuardian/ChargeGrunt");
            walkGruntClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/BellGuardian/WalkGrunt");
            meleeGruntClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/BellGuardian/MeleeGrunt");
            meleeSwishClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/BellGuardian/MeleeSwish");
            stepClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/BellGuardian/Step");
            slamClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/BellGuardian/Slam");
            bellClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/BellGuardian/Bell");

        }
    }

    #region Jump and Slam

    public void Jump()
    {
        inAir = true;
        agent.enabled = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = new Vector3(0, 8f, 0); // ~2s total airtime
    }

    // Animation Event: "Slam" (Landing moment)
    public void Slam()
    {
        rb.isKinematic = true;
        agent.enabled = true;
        rb.useGravity = false;
        agent.Warp(transform.position);
        inAir = false;
        isAttacking = false;
        AudioSystem.Instance.PlayClipFollow(slamClip, transform, 1f);

        // Play slam VFX
        GameObject impactPrefab = Resources.Load<GameObject>("3D/Enemies/GroundEnemies/BellGuardian/Impact");
        Vector3 groundPos = new Vector3(transform.position.x, agent.nextPosition.y, transform.position.z);
        GameObject impact = Instantiate(impactPrefab, groundPos, Quaternion.identity);
        Destroy(impact, 3f);

        //  Create expanding damage hitbox
        GameObject hitbox = new GameObject("SlamHitbox");
        hitbox.transform.SetParent(transform);                // Attach to enemy
        hitbox.transform.localPosition = Vector3.zero;        // Center at feet


        var h = hitbox.AddComponent<UniversalHitCollider>();
        h.Initialize(
            transform,
            UniversalHitCollider.HitboxType.Expanding,
            20f,     // damage
            7f,      // radius
            0.35f    // duration
        );
    }

    #endregion

    #region Melee Attack

    public void MeleeSwish()
    {
        GameObject hitObj = new GameObject("HandHitbox");
        var hit = hitObj.AddComponent<UniversalHitCollider>();
        AudioSystem.Instance.PlayClipFollow(meleeSwishClip, transform, 1f);

        hit.Initialize(
            leftHandBone.transform,                           // parent to hand
            UniversalHitCollider.HitboxType.Static,
            20f,                                      // damage
            1.2f,                                     // radius
            0.5f                                     // duration
        );

        isAttacking = false;
    }

    public void MeleeGrunt() => AudioSystem.Instance.PlayClipFollow(meleeGruntClip, transform, 1f);
    public void ChargeGrunt() => AudioSystem.Instance.PlayClipFollow(chargeGruntClip, transform, 1f);

    #endregion

    #region Walking and Idle Sounds

    public void Bell() => AudioSystem.Instance.PlayClipFollow(bellClip, transform, 1f);

    private void Step()
    {
        AudioSystem.Instance.PlayClipFollow(stepClip, transform, 1f);
        stepsUntilGrunt--;
        if (stepsUntilGrunt == 0)
        {
            AudioSystem.Instance.PlayClipFollow(walkGruntClip, transform, 1f);
            stepsUntilGrunt = Random.Range(8, 16);
        }
    }

    public void IdleGrunt()
    {
        idleUntilGrunt--;
        if (idleUntilGrunt == 0)
        {
            AudioSystem.Instance.PlayClipFollow(walkGruntClip, transform, 1f);
            idleUntilGrunt = Random.Range(8, 16);
        }
    }

    #endregion
}
