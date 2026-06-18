using System;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    
    [SerializeField] private EnemyShipSpawner enemyShipSpawner;

    [SerializeField] private EnemyFactionProfile enemyFactionProfile;

    public static event Action<float, float> OnPlayerShieldDamaged;
    public static event Action<float, float> OnPlayerShieldRepaired;
    public static event Action OnPlayerShieldBreak;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning($"Duplicate CombatManager found on {gameObject.name}! Destroying the duplicate.");
            Destroy(gameObject);
        }
    }
    void Start()
    {
        enemyShipSpawner.SetActiveFaction(enemyFactionProfile);
        enemyShipSpawner.SpawnEnemyShip();
    }
    
    public void EnemyShipAttacksPlayerShip(float weaponDamage)
    {
        float shieldHP = RunManager.Instance.currentShieldHealth;
        float shieldMaxHP = RunManager.Instance.maxShieldHealth;

        Debug.Log($"[CombatManager] shieldHP = {shieldHP} shieldMaxHP = {shieldMaxHP}");
        bool shotMissed = CheckIfDodged();

        if (shotMissed)
        {
            Debug.Log("[EnemyCombatStation] Shot missed!");
            return;
        }
        
        if (shieldHP > 0)
        {
            float shieldDamage = Mathf.Min(weaponDamage, shieldHP);
            shieldHP -= shieldDamage;
            weaponDamage -= shieldDamage;

            OnPlayerShieldDamaged?.Invoke(shieldHP, shieldMaxHP);

            if (shieldHP <= 0)
            {
                shieldHP = 0;
                OnPlayerShieldBreak?.Invoke();
            }
            RunManager.Instance.currentShieldHealth = shieldHP;
        }

        if (weaponDamage > 0)
        {
            CameraShake.Instance.TriggerShake(0.2f, 8f);
            RunManager.Instance.DamageShip((int)weaponDamage);
        }
    }

    
    public bool CheckIfDodged()
    {
        float currentDodgeChance = RunManager.Instance.shipDodgeChance;
        float randomRoll = UnityEngine.Random.Range(0f, 100f);


        if (RunManager.Instance.isCloaked)
        {
            currentDodgeChance += RunManager.Instance.additionalDodgeChanceFromCloak;
        }
        Debug.Log($"[CombatManager] Dodge Chance = {currentDodgeChance} Roll = {randomRoll}");
        Debug.Log($"[CombatManager] {currentDodgeChance >= randomRoll}");
        return currentDodgeChance >= randomRoll;
    }
}
