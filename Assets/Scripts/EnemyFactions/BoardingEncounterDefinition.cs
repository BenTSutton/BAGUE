using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Boarding Encounter")]
public class BoardingEncounterDefinition : ScriptableObject
{
    [Min(0)]
    public int totalEnemies = 5;

    [Min(0)]
    public int maximumAliveEnemies = 3;

    [Header("Random spawn interval")]
    [Min(0)]
    public float minimumSpawnInterval = 2f;

    [Min(0)]
    public float maximumSpawnInterval = 5f;

    [Header("Possible enemy types")]
    public List<EnemyDefinition> enemyTypes;
}