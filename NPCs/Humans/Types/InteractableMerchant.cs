/* using UnityEngine;

[RequireComponent(typeof(HumanMover))]
[RequireComponent(typeof(Animator))]
public class InteractableMerchant : InteractableNPC
{
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private bool returningToIdle = false;

    void Start()
    {
        originalPosition = this.transform.position;
    }

    void LateUpdate()
    {
        if (isDead || angry)
            return;

        if (NPCInteractionSystem.Instance.currentNPC == this)
        {
            agent.isStopped = true;
            FacePlayer();
            humanMover.SetIsMoving(false);
            returningToIdle = false;
        }
        else
        {
            float distance = Vector3.Distance(transform.position, originalPosition);

            if (!returningToIdle && distance > 0.5f)
            {
                returningToIdle = true;
                agent.isStopped = false;
                agent.SetDestination(originalPosition);
                humanMover.SetIsMoving(true);
            }
            else if (returningToIdle && distance <= agent.stoppingDistance)
            {
                agent.ResetPath();
                humanMover.SetIsMoving(false);
                transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * 5f);
                returningToIdle = false;
            }
        }
    }
}
*/