using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;

    private static readonly int DoAttack = Animator.StringToHash("doAttack"); // CHANGED
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int IsDead   = Animator.StringToHash("isDead");

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TriggerAttack()          => animator.SetTrigger(DoAttack); // CHANGED
    public void SetMoving(bool value)    => animator.SetBool(IsMoving, value);
    public void TriggerDeath()           => animator.SetTrigger(IsDead);
}