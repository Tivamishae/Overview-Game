using UnityEngine;

public class HumanMover : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetIsMoving(bool moving) => animator.SetBool("isMovement", moving);
    public void SetIsRunning(bool running) => animator.SetBool("isRunning", running);
    public void SetIsIdle(bool idle) => animator.SetBool("isIdle", idle);
    public void SetIsWorking(bool working) => animator.SetBool("isWorking", working);
    public void SetIsAngry(bool angry) => animator.SetBool("isAngry", angry);
    public void TriggerAttack() => animator.SetTrigger("isAttack");
    public void TriggerDeath() => animator.SetTrigger("isDead");
    public void TriggerThrow() => animator.SetTrigger("isThrow");
    public void TriggerTakeDamage() => animator.SetTrigger("isHit");
}
