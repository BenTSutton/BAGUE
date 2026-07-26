using UnityEngine;
using System.Collections;

public enum EnemyAttackTarget
{
    None,
    Player,
    Room
}
public class EnemyAI : MonoBehaviour
{
    public Transform player;

    [Header("Movement")]
    public float speed = 3f;
    public float chaseDistance = 6f;
    public float attackDistance = 1.5f;

    // Patrol points
    public Transform pointA;
    public Transform pointB;

    public bool isKnockedBack = false;

    [Header("States")]
    public EnemyState currentState;

    public IdleState idleState;
    public PatrolState patrolState;
    public ChaseState chaseState;
    public AttackState attackState;

    [Header("Animation")]
    public EnemyAnimator enemyAnimator;

    [Header("Objective")]
    public RoomHealth assignedRoom;
    private EnemyDefinition definition;

    public int playerDamage => definition.playerDamage;
    public int roomDamage => definition.roomDamage;
    public float attackCooldown => definition.attackInterval;
    public float attackWindup => definition.attackWindup;

    public void Initialize(EnemyDefinition enemyDefinition, RoomHealth room)
    {
        if (enemyDefinition == null)
        {
            Debug.LogError("Enemy initialized without a definition.", this);
            return;
        }

        definition = enemyDefinition;
        assignedRoom = room;

        speed = definition.moveSpeed;
        chaseDistance = definition.chaseDistance;
        attackDistance = definition.attackDistance;

        EnemyHealth health = GetComponent<EnemyHealth>();

        if (health != null)
            health.Initialize(definition.maxHealth);
    }

    private void Start()
    {
        if (definition == null)
        {
            Debug.LogError(
                $"{name} was spawned without EnemyAI.Initialize().",
                this);

            enabled = false;
            return;
        }

        enemyAnimator = GetComponent<EnemyAnimator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        idleState = new IdleState(this);
        patrolState = new PatrolState(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);

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

    public IEnumerator KnockbackPause()
    {
        isKnockedBack = true;
        yield return new WaitForSeconds(0.2f);
        isKnockedBack = false;
    }

    public EnemyAttackTarget SelectAttackTarget()
    {
        bool playerAttackable =
            player != null &&
            DistanceTo(player) <= attackDistance;

        bool playerInChaseRange = IsPlayerInChaseRange();

        bool roomAttackable =
            assignedRoom != null &&
            !assignedRoom.IsDestroyed &&
            DistanceTo(assignedRoom.transform) <= attackDistance;

        switch (definition.objectivePriority)
        {
            case EnemyObjectivePriority.Adaptive:
            case EnemyObjectivePriority.PlayerFirst:
                if (playerAttackable)
                    return EnemyAttackTarget.Player;

                if (playerInChaseRange)
                    return EnemyAttackTarget.None;

                if (roomAttackable)
                    return EnemyAttackTarget.Room;
                break;

            case EnemyObjectivePriority.RoomFirst:
                if (roomAttackable)
                    return EnemyAttackTarget.Room;

                if (playerAttackable)
                    return EnemyAttackTarget.Player;
                break;

            case EnemyObjectivePriority.PlayerOnly:
                if (playerAttackable)
                    return EnemyAttackTarget.Player;
                break;

            case EnemyObjectivePriority.RoomOnly:
                if (roomAttackable)
                    return EnemyAttackTarget.Room;
                break;
        }

        return EnemyAttackTarget.None;
    }

    public Transform SelectMovementTarget()
    {
        bool roomAvailable =
            assignedRoom != null &&
            !assignedRoom.IsDestroyed;

        bool playerInChaseRange = IsPlayerInChaseRange();

        switch (definition.objectivePriority)
        {
            case EnemyObjectivePriority.RoomFirst:
            case EnemyObjectivePriority.RoomOnly:
                if (roomAvailable)
                    return assignedRoom.transform;

                if (definition.objectivePriority == EnemyObjectivePriority.RoomFirst &&
                    playerInChaseRange)
                {
                    return player;
                }

                return null;

            case EnemyObjectivePriority.Adaptive:
            case EnemyObjectivePriority.PlayerFirst:
                if (playerInChaseRange)
                    return player;

                return roomAvailable
                    ? assignedRoom.transform
                    : null;

            case EnemyObjectivePriority.PlayerOnly:
                return playerInChaseRange
                    ? player
                    : null;
        }

        return null;
    }

    public bool HasPlayerChaseTarget()
    {
        return player != null && SelectMovementTarget() == player;
    }

    private bool IsPlayerInChaseRange()
    {
        return player != null &&
            Vector2.Distance(transform.position, player.position)
                <= chaseDistance;
    }

    private float DistanceTo(Transform target)
    {
        Collider2D targetCollider = target.GetComponent<Collider2D>();

        if (targetCollider == null)
            targetCollider = target.GetComponentInChildren<Collider2D>();

        Vector2 targetPosition = targetCollider != null
            ? targetCollider.ClosestPoint(transform.position)
            : target.position;

        return Vector2.Distance(transform.position, targetPosition);
    }
}
