using UnityEngine;
using System;
using UnityEngine.Serialization;

public class CombatManager : MonoBehaviour
{
    [SerializeField]
    private EnemyShipSpawner enemyShipSpawner;

    [SerializeField] private EnemyFactionProfile debugFallbackFaction;

    [SerializeField, Min(1f)]
    private float desperationWeaponRechargeMultiplier = 1.2f;

    [Header("Enemy Desperation")]
    [SerializeField, Range(0.01f, 0.99f)]
    private float desperationHullThreshold = 0.45f;
    [SerializeField]
    private BoardingController boardingController;

    /// First value: whether desperation is active.
    /// Second value: whether the engine was operational when it began.
    public static event Action<bool, bool>
        EnemyDesperationChanged;

    private EnemyShip activeEnemyShip;
    private EnemyCombatStation weaponStation;
    private EnemyLaunchBayStation launchBayStation;
    private EnemyEngineStation engineStation;

    private bool desperationTriggered;
    private bool desperationActive;
    private bool combatEnding;

    private void OnEnable()
    {
        EnemyShip.OnEnemyShipSpawn += HandleEnemyShipSpawned;
        EnemyEngineStation.EnemyEscaped += HandleEnemyEscape;
        GameManager.CombatResolutionStarted += HandleCombatResolutionStarted;
    }

    private void OnDisable()
    {
        EnemyShip.OnEnemyShipSpawn -= HandleEnemyShipSpawned;
        EnemyEngineStation.EnemyEscaped -= HandleEnemyEscape;
        GameManager.CombatResolutionStarted -= HandleCombatResolutionStarted;

        StopDesperation();
        UnbindEnemyShip();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (enemyShipSpawner == null)
        {
            Debug.LogError("[CombatManager] EnemyShipSpawner is missing.", this);

            return;
        }

        EnemyFactionProfile activeFaction = ResolveActiveFaction();

        if (activeFaction == null)
        {
            Debug.LogError("[CombatManager] No enemy faction could be resolved.", this);

            return;
        }

        ConfigureBoardingEncounter(activeFaction);

        enemyShipSpawner.SetActiveFaction(activeFaction);
        enemyShipSpawner.SpawnEnemyShip();
    }

    private EnemyFactionProfile ResolveActiveFaction()
    {
        CombatDefinition combatDefinition = GameManager.Instance != null
            ? GameManager.Instance.currentCombatNode
            : null;

        if (combatDefinition != null && combatDefinition.EnemyFaction != null)
        {
            return combatDefinition.EnemyFaction;
        }

        if (RunManager.Instance != null && RunManager.Instance.enemyFaction != null)
        {
            Debug.LogWarning("[CombatManager] CombatDefinition has no faction. " + "Using the run faction.", this);

            return RunManager.Instance.enemyFaction;
        }

        if (debugFallbackFaction != null)
        {
            Debug.LogWarning("[CombatManager] Using the combat-scene debug faction.", this);
        }

        return debugFallbackFaction;
    }

    private void ConfigureBoardingEncounter(EnemyFactionProfile activeFaction)
    {
        if (boardingController == null)
        {
            Debug.LogWarning("[CombatManager] BoardingController is missing.", this);

            return;
        }

        if (activeFaction.BoardingEncounterDefinitions == null || activeFaction.BoardingEncounterDefinitions.Count == 0)
        {
            Debug.LogWarning("[CombatManager] Active faction has no boarding encounter.", this);

            return;
        }

        boardingController.ConfigureEncounter(activeFaction.BoardingEncounterDefinitions[0]);
    }

    private void HandleEnemyShipSpawned(EnemyShip enemyShip)
    {
        UnbindEnemyShip();

        if (enemyShip == null)
        {
            return;
        }

        activeEnemyShip = enemyShip;

        weaponStation = enemyShip.GetComponentInChildren<EnemyCombatStation>(true);

        launchBayStation = enemyShip.GetComponentInChildren<EnemyLaunchBayStation>(true);

        engineStation = enemyShip.GetComponentInChildren<EnemyEngineStation>(true);

        activeEnemyShip.OnEnemyShipHPChange +=HandleEnemyHullChanged;

        EvaluateDesperationThreshold();
    }

    private void UnbindEnemyShip()
    {
        if (activeEnemyShip != null)
        {
            activeEnemyShip.OnEnemyShipHPChange -= HandleEnemyHullChanged;
        }

        activeEnemyShip = null;
        weaponStation = null;
        launchBayStation = null;
        engineStation = null;
    }

    private void HandleEnemyHullChanged()
    {
        EvaluateDesperationThreshold();
    }

    private void EvaluateDesperationThreshold()
    {
        if (desperationTriggered ||
            combatEnding ||
            activeEnemyShip == null)
        {
            return;
        }

        float maximumHealth = activeEnemyShip.GetShipMaxHealth;
        float currentHealth = activeEnemyShip.GetShipHealth;

        if (maximumHealth <= 0f || currentHealth <= 0f)
        {
            return;
        }

        float hullPercentage = currentHealth / maximumHealth;

        if (hullPercentage > desperationHullThreshold)
        {
            return;
        }

        TriggerDesperation();
    }

    private void TriggerDesperation()
    {
        if (desperationTriggered || combatEnding)
        {
            return;
        }

        desperationTriggered = true;
        desperationActive = true;

        bool weaponOperational = weaponStation != null && !weaponStation.IsBroken;

        bool launchBayOperational = launchBayStation != null && launchBayStation.CanLaunchWaves;

        bool engineOperational = engineStation != null && !engineStation.IsBroken;

        if (weaponOperational)
        {
            weaponStation.SetRechargeRateMultiplier(desperationWeaponRechargeMultiplier);
        }

        // Raise this before requesting the wave so the alarm/message
        // begins before the boarding warning.
        EnemyDesperationChanged?.Invoke(true, engineOperational);

        if (launchBayOperational)
        {
            if (boardingController != null)
            {
                boardingController.RequestFinalWave();
            }
            else
            {
                Debug.LogWarning("[CombatManager] BoardingController is missing. " + "Final desperation wave skipped.", this);
            }
        }
        else
        {
            Debug.Log("[CombatManager] Launch bay destroyed. " + "Final desperation wave skipped.", this);
        }

        Debug.Log("[CombatManager] Enemy systems overloading!", this);
    }

    private void HandleCombatResolutionStarted()
    {
        combatEnding = true;
        StopDesperation();

        if(weaponStation != null) 
        {
            weaponStation.enabled = false;
        }

        if(engineStation != null) 
        {
            engineStation.enabled = false;
        }

        boardingController?.StopEncounter();
    }

    private void StopDesperation()
    {
        if(!desperationActive) 
        {
            return;
        }

        desperationActive = false;

        if(weaponStation != null && !weaponStation.IsBroken) 
        {
            weaponStation.SetRechargeRateMultiplier(1f);
        }

        boardingController?.StopEncounter();
        EnemyDesperationChanged?.Invoke(false, false);
    }

    private void HandleEnemyEscape()
    {
        Debug.Log("[CombatManager] Resolving enemy escape.", this);

        combatEnding = true;
        StopDesperation();

        GameManager.Instance?.ResolveEnemyEscape();
    }
}
