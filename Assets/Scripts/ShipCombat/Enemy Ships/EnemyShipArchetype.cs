using UnityEngine;

[CreateAssetMenu(fileName = "NewShipArchetype", menuName = "EnemyShipSystem/Ship Archetypes")]
public class EnemyShipArchetype : ScriptableObject
{
    [Header("Archetype Identity")]
    public string archetypeName;
    
    [Header("Weapon Stats")]
    public float weaponDamage = 10f;
    // public float weaponRechargeRate = 1.0f;

    [Header("Defense Stats")]
    public float maxHealth = 10f;
    public float maxShields = 10f;
    // public float shieldRegenRate = 5f;
    // public float maxHullHealth = 150f;

    // public EnemyCombatStation signatureCombatStation;
}