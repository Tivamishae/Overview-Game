using UnityEngine;

public class ArmAnimatorScript : StateMachineBehaviour
{
    private ArmMovements arm;

    // Called when entering the state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Cache reference ONCE
        if (arm == null)
            arm = animator.GetComponent<ArmMovements>();
    }

    // Called when exiting the state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (arm != null)
            arm.ActionFinished();
    }
}
