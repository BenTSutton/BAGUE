using UnityEngine;
using System.Collections;

public class AttackState : EnemyState
{
    private float attackCooldown = 1f;
    private float attackTimer = 0f;
    private float windupDuration = 0.6f; 
    private bool isWindingUp = false;
    private Coroutine windupRoutine;  

    public AttackState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        attackTimer = attackCooldown;
        enemy.enemyAnimator.SetMoving(false);
    }

    public override void Exit()
    {
        isWindingUp = false;

        
        if (windupRoutine != null) //PLEASE FUCKING WORK (cancel chase fix)
        {
            enemy.StopCoroutine(windupRoutine);
            windupRoutine = null;
        }

        enemy.GetComponent<Animator>().ResetTrigger("doAttack");
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
        windupRoutine = enemy.StartCoroutine(WindupAttack());
        attackTimer = attackCooldown;
    }

    // Only exit if not mid-attack
    if (distance > enemy.attackDistance && !isWindingUp)
    {
        enemy.ChangeState(enemy.chaseState);
    }
}

        private IEnumerator WindupAttack()
    {
        isWindingUp = true;
        enemy.enemyAnimator.TriggerAttack();
        yield return new WaitForSeconds(windupDuration);

        PlayerHealth playerHealth = enemy.player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1, enemy.transform); 
        }

        isWindingUp = false;
    }
}