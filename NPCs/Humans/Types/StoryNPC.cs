using UnityEngine;

public class StoryNPC : InteractableNPC
{
    void LateUpdate()
    {
        if (isDead || angry)
            return;

        if (isBeingInteractedWith)
        {
            humanMover.SetIsWorking(false);
            agent.isStopped = true;
            FacePlayer();
            return;
        }
    }
}
