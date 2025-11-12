using UnityEngine;

public class RangedAttack : Attack
{
    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private string animationTrigger = "Attack";
    public override string AnimationTrigger => animationTrigger;

    protected virtual void Awake()
    {
        AngerReaction = (npc) => AngerReactions.Instance.ChaseReaction(npc, range);
    }

    public override void Execute(NPC npc)
    {

        AngerReaction?.Invoke(npc);
        npc.PlayTrigger(animationTrigger);

        if (projectilePrefab != null && shootPoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

            if (proj.TryGetComponent(out Rigidbody rb))
            {
                Vector3 dir = (npc.player.transform.position + Vector3.up * 1.2f - shootPoint.position).normalized;
                rb.linearVelocity = dir * projectileSpeed;
            }
        }
    }
}

