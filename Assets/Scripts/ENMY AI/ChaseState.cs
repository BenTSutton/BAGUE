using UnityEngine;

public class ChaseState : EnemyState
{
    public ChaseState(EnemyAI enemy) : base(enemy) { }

    public override void Update()
    {
        float distance = Vector2.Distance(
            enemy.transform.position,
            enemy.player.position
        );

        enemy.transform.position = Vector2.MoveTowards(
            enemy.transform.position,
            enemy.player.position,
            enemy.speed * Time.deltaTime
        );

        if (distance > enemy.chaseDistance)
        {
            enemy.ChangeState(enemy.idleState);
        }
    }
}