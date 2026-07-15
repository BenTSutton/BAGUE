using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShipArchetype", menuName = "EnemyShipSystem/Ship Archetypes")]
public class EnemyShipArchetype : ScriptableObject
{
    [Header("Archetype Identity")]
    public string archetypeName;
    
    [Header("Weapon Stats")]
    public int weaponDamage = 10;
    // public float weaponRechargeRate = 1.0f;

    [Header("Defense Stats")]
    public float maxHealth = 10f;
    public float maxShields = 10f;

    // public float shieldRegenRate = 5f;
    [Header("Station Rules")]
    [Tooltip("Stations that this archetype is guaranteed to spawn.")]
    [SerializeField] private List<EnemyShipStation> guaranteedStations;
    [SerializeField] private List<EnemyShipStation> randomStationPool;

    public List<EnemyShipStation> GuaranteedStations => guaranteedStations;
    public List<EnemyShipStation> RandomStationPool => randomStationPool;

}