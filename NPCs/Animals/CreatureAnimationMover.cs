using UnityEngine;

public class CreatureAnimationMover : MonoBehaviour
{
    public Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetIsMoving(bool action) {animator.SetBool("isMoving", action);}
    public void SetIsRunning(bool action) { animator.SetBool("isRunning", action); }
    public void PlayHitTrigger() { animator.SetTrigger("hitTrigger"); }
    public void PlayDeathTrigger() { animator.SetTrigger("deathTrigger"); }
    public void SetIsAttacking(bool action) { animator.SetBool("isAttacking", action); }
}
