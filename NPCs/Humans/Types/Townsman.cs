using UnityEngine;

public class Townsman : Villager
{
    [Header("Work")]
    public NPCWorkArea Work;

    protected override void Update()
    {
        base.Update();

        if (currentState == NPCState.Dead)
            return;

        if (currentState == NPCState.Angry)
            return;

        HandleWorkBehavior();
    }

    private void HandleWorkBehavior()
    {
        if (Work == null)
        {
            if (idleReaction != null)
                idleReaction.ExecuteIdle(this);
            return;
        }

        Vector3 targetPos = Work.GetStandingPosition();
        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance > 1f)
        {
            MoveTowards(Work.finalGroundPosition);
            PlayTrigger("Walking");
            animator.SetBool("Working", false);
        }
        else
        {
            FaceTarget(Work.transform.position);
            StopMoving();
            PlayTrigger("Working");
            animator.SetBool("Working", true);
        }
    }

    public override void ResetEnemy()
    {
        base.ResetEnemy();
        Work = null;
    }
}
