using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyFactionData", menuName = "EnemyFaction", order = 1)]
public class EnemyFactionProfile : ScriptableObject
{
    [Header("Faction Identity")]
    [SerializeField] private string factionName;

    [Header("Prefab of their ship")]
    [SerializeField] private GameObject enemyShipPrefab;

    [Header("List of all possible enemy ship archetypes for faction")]
    [SerializeField] private List<EnemyShipArchetype> enemyShipArchetypeList;

    public List<EnemyShipArchetype> EnemyShipArchetypeList => enemyShipArchetypeList;

    public string FactionName => factionName;
    public GameObject EnemyShipPrefab => enemyShipPrefab;
}
