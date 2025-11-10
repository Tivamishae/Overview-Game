using UnityEngine;

public class MeleeAttack : Attack
{
    [SerializeField] private string animationTrigger = "Attack";
    public override string AnimationTrigger => animationTrigger;

    protected void Awake()
    {
        // Default chase behavior
        AngerReaction = (npc) => AngerReactions.Instance.ChaseReaction(npc, range);
    }

    protected override void PerformAttack(NPC npc)
    {
        // Default melee attack: spawn hitbox
        CreateHitbox();
    }
}
