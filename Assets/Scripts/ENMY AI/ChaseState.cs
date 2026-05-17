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
        float distance = Vector2.Distance(
            enemy.transform.position,
            enemy.player.position
        );

        if (!enemy.isKnockedBack)
        {
            enemy.transform.position = Vector2.MoveTowards(
                enemy.transform.position,
                enemy.player.position,
                enemy.speed * Time.deltaTime
            );
        }

        if (distance < enemy.attackDistance)
        {
            enemy.ChangeState(enemy.attackState);
            return;
        }

        if (distance > enemy.chaseDistance)
        {
            enemy.ChangeState(enemy.patrolState);
        }
        
        Vector2 direction = (enemy.player.position - enemy.transform.position).normalized;
        if (direction.x != 0)
        {
            enemy.GetComponent<SpriteRenderer>().flipX = direction.x > 0;
        }
    }
}