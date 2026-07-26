using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnemyFactionData", menuName = "EnemyFaction", order = 1)]
public class EnemyFactionProfile : ScriptableObject
{
    [Header("Faction Identity")]
    [SerializeField] private string factionName;

    [Header("Prefab of their ship")]
    [SerializeField] private GameObject enemyShipPrefab;
    [Header("Boarding Encounter Definitions for this faction")]
    [SerializeField] private List<BoardingEncounterDefinition> boardingEncounterDefinitions;

    public string FactionName => factionName;
    public GameObject EnemyShipPrefab => enemyShipPrefab;
    public List<BoardingEncounterDefinition> BoardingEncounterDefinitions => boardingEncounterDefinitions;
}
