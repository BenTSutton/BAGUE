using UnityEngine;

public enum EnemyObjectivePriority
{
    Adaptive, 
    PlayerFirst,
    RoomFirst,
    PlayerOnly,
    RoomOnly
}

[CreateAssetMenu(menuName = "Enemies/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Prefab")]
    public GameObject prefab;

    [Header("Objective")]
    public EnemyObjectivePriority objectivePriority;

    [Header("Combat")]
    public int maxHealth = 3;
    public int playerDamage = 1;
    public int roomDamage = 5;
    public float attackInterval = 2f;
    public float attackWindup = 0.6f;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float chaseDistance = 6f;
    public float attackDistance = 1.5f;
}
