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

    public bool isKnockedBack = false; // ADD THIS
    private bool isStunned;
    [Header("States")]
    //States
    private EnemyState currentState;

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

    public Rigidbody2D Body { get; private set; }

    private void Awake()
    {
        Body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }

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
        // Deck Room level affects starting health of boarders
        DeckRoom deckRoom = RunManager.Instance.GetRoomData<DeckRoom>();
        int deckLevel = RunManager.Instance.GetRoomLevel<DeckRoom>();
        int healthReduction = deckRoom.GetBoarderHealthReduction(deckLevel);

        if (health != null)
        {
            health.Initialize(Mathf.Max(definition.maxHealth - healthReduction, 1));
        }

        // Deck Room stuns boarders if levelled up 
        float stunDuration = deckRoom.GetBoarderEntryStunDuration(deckLevel);

        if (stunDuration > 0f)
        {
            Stun(stunDuration);
        }

        // Again, Deck Room deals damage to boarders periodically
        int periodicDamage = deckRoom.GetPeriodicDamage(deckLevel);

        if (periodicDamage > 0)
        {
            StartCoroutine(ApplyPeriodicDeckDamage(
                periodicDamage,
                deckRoom.GetPeriodicDamageInterval()));
        }
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
        if (isStunned)
        {
            return;
        }

        currentState.Update();
    }

    public void ChangeState(EnemyState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    public IEnumerator KnockbackPause(float duration)
    {
        isKnockedBack = true;

        yield return new WaitForSeconds(duration);

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
    
    public void Stun(float duration)
    {
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    private IEnumerator ApplyPeriodicDeckDamage(int damage, float interval)
    {
        EnemyHealth health = GetComponent<EnemyHealth>();

        while (true)
        {
            yield return new WaitForSeconds(interval);
            health.TakeDamage(damage);
        }
    }

    public void AttackHit()
    {
        if(currentState == attackState)
        {
            attackState.EnemyAttack();
        }
    }

    public void FinishAttack()
    {
        if(currentState == attackState)
        {
            attackState.FinishEnemyAttack();
        }
    }

    public void FaceDirection(float horizontalDirection)
    {
        if (Mathf.Abs(horizontalDirection) < 0.01f)
            return;

        SpriteRenderer enemySprite = GetComponent<SpriteRenderer>();

        if (enemySprite != null)
            enemySprite.flipX = horizontalDirection > 0f;
    }

    public bool HasMovementTarget()
    {
        return SelectMovementTarget() != null;
    }
}
