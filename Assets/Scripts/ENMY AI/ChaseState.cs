using UnityEngine;

public class ChaseState : EnemyState
{
    public ChaseState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        if (enemy.HasPlayerChaseTarget())
            SFXManager.Instance?.PlayEnemySpotPlayer(enemy.transform.position);

        enemy.enemyAnimator.SetMoving(true);  
    }

    public override void Exit()
    {
        enemy.enemyAnimator.SetMoving(false);

        if (!enemy.isKnockedBack)
        {
            enemy.Body.linearVelocity = new Vector2(0f, enemy.Body.linearVelocity.y);
        }
    }

    private Transform movementTarget;

    public override void Update()
    {
        movementTarget = enemy.SelectMovementTarget();

        if (movementTarget == null)
        {
            enemy.ChangeState(enemy.idleState);
            return;
        }

        if (enemy.SelectAttackTarget() != EnemyAttackTarget.None)
        {
            enemy.ChangeState(enemy.attackState);
            return;
        }

        float direction = movementTarget.position.x - enemy.transform.position.x;

        if (Mathf.Abs(direction) > 0.01f)
            enemy.FaceDirection(direction);
    }

    public override void FixedUpdate()
    {
        if (movementTarget == null || enemy.isKnockedBack)
            return;

        float direction = Mathf.Sign(movementTarget.position.x - enemy.transform.position.x);

        enemy.Body.linearVelocity = new Vector2(direction * enemy.speed, enemy.Body.linearVelocity.y);
    }
}
