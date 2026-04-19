using UnityEngine;
using System.Collections;

public class AttackState : EnemyState
{
    private float attackCooldown = 1f;
    private float attackTimer = 0f;
    private float windupDuration = 0.3f; 
    private bool isWindingUp = false;

    public AttackState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
{
    attackTimer = attackCooldown; //now hopefully his damn attacks will work... jeepers
    enemy.enemyAnimator.SetMoving(false);
}

    public override void Exit()
    {
        isWindingUp = false;
        enemy.StopAllCoroutines();
        enemy.GetComponent<Animator>().ResetTrigger("doAttack"); // clears queued trigger
    }

    public override void Update()
{
    float distance = Vector2.Distance(
        enemy.transform.position,
        enemy.player.position
    );

    attackTimer -= Time.deltaTime;

    if (attackTimer <= 0f && !isWindingUp)
    {
        enemy.StartCoroutine(WindupAttack());
        attackTimer = attackCooldown;
    }

    // Only exit if not mid-attack
    if (distance > enemy.attackDistance && !isWindingUp)
    {
        enemy.ChangeState(enemy.chaseState);
    }
}

    private IEnumerator WindupAttack() //AI suggested this fix.
{
    isWindingUp = true;
    enemy.enemyAnimator.TriggerAttack();
    yield return new WaitForSeconds(windupDuration);

    
    PlayerHealth playerHealth = enemy.player.GetComponent<PlayerHealth>();
    if (playerHealth != null)  
    {
        playerHealth.TakeDamage(1, enemy.transform.position);
    }

        isWindingUp = false;
    }
}