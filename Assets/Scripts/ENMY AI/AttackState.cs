using System.Collections;
using UnityEngine;

public class AttackState : EnemyState
{
    private float attackTimer;
    private bool isWindingUp;
    private Coroutine windupRoutine;
    private EnemyAttackTarget intendedAttackTarget;

    public AttackState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        attackTimer = 0f;
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
            WindupAttack(target);
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

        enemy.GetComponent<Animator>().ResetTrigger("doAttack");
    }

    private void WindupAttack(EnemyAttackTarget intendedTarget)
    {
        isWindingUp = true;
        intendedAttackTarget = intendedTarget;
        enemy.enemyAnimator.TriggerAttack();
    }

    public void EnemyAttack()
    {
        if (!isWindingUp)
            return;

        EnemyAttackTarget currentTarget = enemy.SelectAttackTarget();

        if (currentTarget == intendedAttackTarget)
            ApplyDamage(currentTarget);
    }

    public void FinishEnemyAttack()
    {
        isWindingUp = false;
        attackTimer = enemy.attackCooldown;
    }

    private void ApplyDamage(EnemyAttackTarget target)
    {
        switch (target)
        {
            case EnemyAttackTarget.Player:
                PlayerHealth playerHealth = enemy.player.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {
                    Debug.Log("Applying damage to player");
                    playerHealth.TakeDamage(enemy.playerDamage, enemy.transform);
                }
                break;

            case EnemyAttackTarget.Room:
                Debug.Log("Applying damage to Room");
                enemy.assignedRoom?.TakeDamage(enemy.roomDamage);
                break;
        }
    }
}
