using UnityEngine;

public class PatrolState : EnemyState
{
    private Transform targetPoint;

    public PatrolState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        if (enemy.pointA != null)
        targetPoint = enemy.pointA;

        enemy.enemyAnimator.SetMoving(true);
    }
    public override void Exit()
    {
        enemy.enemyAnimator.SetMoving(false);
    }

    public override void Update()
    {
        if (enemy.pointA == null || enemy.pointB == null)
        {
            Debug.LogError("Patrol points not assigned on " + enemy.gameObject.name);
            return;
        }

        float distanceToPlayer = Vector2.Distance(
            enemy.transform.position,
            enemy.player.position
        );

        if (distanceToPlayer < enemy.chaseDistance)
        {
            enemy.ChangeState(enemy.chaseState);
            return;
        }

        if (!enemy.isKnockedBack)
        {
            enemy.transform.position = Vector2.MoveTowards(
                enemy.transform.position,
                targetPoint.position,
                enemy.speed * Time.deltaTime
            );

           Vector2 direction = (targetPoint.position - enemy.transform.position).normalized;
            if (direction.x != 0)
            {
                enemy.GetComponent<SpriteRenderer>().flipX = direction.x < 0;
            }
        }
    }
}