using UnityEngine;

public class IdleState : EnemyState
{
    public IdleState(EnemyAI enemy) : base(enemy) { }

    public override void Update()
    {
        if (enemy.SelectAttackTarget() != EnemyAttackTarget.None)
        {
            enemy.ChangeState(enemy.attackState);
            return;
        }

        if (enemy.HasMovementTarget())
            enemy.ChangeState(enemy.chaseState);
    }
}
