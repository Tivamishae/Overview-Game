using UnityEngine;

public class Townsman : InteractableNPC
{
    public NPCWorkArea Work;

    void LateUpdate()
    {
        if (isDead) return;

        if (angry)
        {
            ChasePlayer();
            return;
        }

        if (Work == null)
        {
            canWander = true;
            return;
        }

        Vector3 targetPos = Work.GetStandingPosition();
        float distance = Vector3.Distance(transform.position, targetPos);
        canWander = false;

        if (distance > 1f)
        {
            agent.SetDestination(Work.finalGroundPosition);
            humanMover.SetIsMoving(true);
            humanMover.SetIsWorking(false);
        }
        else
        {
            FaceWorkBench();
            humanMover.SetIsMoving(false);
            humanMover.SetIsWorking(true);
        }
    }

    private void FaceWorkBench()
    {
        if (Work != null)
        {
            FaceTarget(Work.transform.position);
        }
    }

    public override void ChasePlayer()
    {
        RunFromPlayer();
    }
}
