using UnityEngine;

public class ChaseState : EnemyState
{
    public ChaseState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.enemyAnimator.SetMoving(true);  
    }

    public override void Exit()
    {
        enemy.enemyAnimator.SetMoving(false); 
    }

    public override void Update()
    {
        if (!enemy.HasPlayerChaseTarget())
        {
            enemy.ChangeState(enemy.idleState);
            return;
        }

        // Attack whichever valid target currently has priority.
        if (enemy.SelectAttackTarget() != EnemyAttackTarget.None)
        {
            enemy.ChangeState(enemy.attackState);
            return;
        }

        Transform target = enemy.player;

        if (!enemy.isKnockedBack)
        {
            enemy.transform.position = Vector2.MoveTowards(
                enemy.transform.position,
                target.position,
                enemy.speed * Time.deltaTime
            );
        }

        Vector2 direction =
            (target.position - enemy.transform.position).normalized;

        if (direction.x != 0)
            enemy.GetComponent<SpriteRenderer>().flipX = direction.x > 0;
    }
}
