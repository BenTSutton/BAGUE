using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    //Movment
    public float speed = 3f;
    public float chaseDistance = 6f;
    public float attackDistance = 1.5f;

    // Patrol points
    public Transform pointA;
    public Transform pointB;

    public bool isKnockedBack = false; // ADD THIS
    //States
    private EnemyState currentState;

    public IdleState idleState;
    public PatrolState patrolState;
    public ChaseState chaseState;
    public AttackState attackState;
    //Animation 
    public EnemyAnimator enemyAnimator;

    void Start()
    {
        idleState = new IdleState(this);
        patrolState = new PatrolState(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
        enemyAnimator = GetComponent<EnemyAnimator>();
        ChangeState(patrolState);
    }

    void Update()
    {
        currentState.Update();
    }

    public void ChangeState(EnemyState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    public IEnumerator KnockbackPause() // ADD THIS
    {
        isKnockedBack = true;
        yield return new WaitForSeconds(0.2f);
        isKnockedBack = false;
    }
}