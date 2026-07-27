using System.Collections;
using UnityEngine;

public class AttackState : EnemyState
{
    private float attackTimer;
    private bool isWindingUp;
    private Coroutine windupRoutine;

    public AttackState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        attackTimer = enemy.attackCooldown;
        enemy.enemyAnimator.SetMoving(false);
    }

    public override void Update()
    {
        attackTimer -= Time.deltaTime;

        EnemyAttackTarget target = enemy.SelectAttackTarget();

        if (target == EnemyAttackTarget.None && !isWindingUp)
        {
            enemy.ChangeState(
                enemy.HasPlayerChaseTarget()
                    ? enemy.chaseState
                    : enemy.idleState);
            return;
        }

        if (attackTimer <= 0f && !isWindingUp)
        {
            windupRoutine = enemy.StartCoroutine(
                WindupAttack(target));

            attackTimer = enemy.attackCooldown;
        }
    }

    public override void Exit()
    {
        isWindingUp = false;

        if (windupRoutine != null)
        {
            enemy.StopCoroutine(windupRoutine);
            windupRoutine = null;
        }

        enemy.GetComponent<Animator>()
            .ResetTrigger("doAttack");
    }

    private IEnumerator WindupAttack(EnemyAttackTarget intendedTarget)
    {
        isWindingUp = true;
        enemy.enemyAnimator.TriggerAttack();

        yield return new WaitForSeconds(enemy.attackWindup);

        // Revalidate the target after the windup.
        EnemyAttackTarget currentTarget = enemy.SelectAttackTarget();

        if (currentTarget == intendedTarget)
            ApplyDamage(currentTarget);

        isWindingUp = false;
        windupRoutine = null;
    }

    private void ApplyDamage(EnemyAttackTarget target)
    {
        switch (target)
        {
            case EnemyAttackTarget.Player:
                PlayerHealth playerHealth =
                    enemy.player.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(
                        enemy.playerDamage,
                        enemy.transform);
                }
                break;

            case EnemyAttackTarget.Room:
                enemy.assignedRoom?.TakeDamage(enemy.roomDamage);
                break;
        }
    }
}
