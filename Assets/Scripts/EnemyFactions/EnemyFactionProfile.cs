using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyFactionData", menuName = "EnemyFaction", order = 1)]
public class EnemyFactionProfile : ScriptableObject
{
    [Header("Faction Identity")]
    [SerializeField] private string factionName;

    [Header("Prefab of their ship")]
    [SerializeField] private GameObject enemyShipPrefab;

    public string FactionName => factionName;
    public GameObject EnemyShipPrefab => enemyShipPrefab;
}
