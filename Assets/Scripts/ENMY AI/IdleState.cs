using UnityEngine;

public class IdleState : EnemyState
{
    public IdleState(EnemyAI enemy) : base(enemy) { }

    public override void Update()
    {
        float distance = Vector2.Distance(
            enemy.transform.position,
            enemy.player.position
        );

        if (distance < enemy.chaseDistance)
        {
            enemy.ChangeState(enemy.chaseState);
        }
    }
}
