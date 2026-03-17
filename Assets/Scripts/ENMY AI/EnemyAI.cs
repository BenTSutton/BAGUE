using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;
    public float chaseDistance = 6f;

    private EnemyState currentState;

    public IdleState idleState;
    public ChaseState chaseState;

    void Start()
    {
        idleState = new IdleState(this);
        chaseState = new ChaseState(this);

        ChangeState(idleState);
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
}