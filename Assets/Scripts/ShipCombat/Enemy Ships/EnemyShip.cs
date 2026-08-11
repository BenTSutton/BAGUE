using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public abstract class EnemyShip : MonoBehaviour
{
    [Header("HP Settings")]
    [SerializeField] protected float health;
    [SerializeField] protected float maxHealth;

    [Header("Shield Settings (Only applied if you have a shield station)")]
    [SerializeField] protected float shieldHealth;
    [SerializeField] protected float shieldMaxHealth;

    [Header("Random Station Selection Elements")]
    [Tooltip("The empty UI/Transform slot objects pre-placed on your ship sprite where stations will be generated.")]
    [SerializeField] protected List<Transform> stationSlots;
    [Tooltip("The types of station prefabs that can be used.")]
    [SerializeField] protected List<EnemyShipStation> availableStationPrefabs;

    protected List<EnemyShipStation> spawnedStations = new List<EnemyShipStation>();

    protected Sprite healthDisplaySprite;
    
    // Has a shield is turned off by default and if the ship has a EnemyShieldStation then it will be enabled by that station.
    public bool hasAShieldStation { get; private set; } = false;
    protected Transform player;

    public event Action OnEnemyShieldBreak;
    public event Action OnEnemyShipHPChange;
    public static event Action<EnemyShip> OnEnemyShipSpawn;
    public static event Action<EnemyShip> OnEnemyShipDeath;
    public static event Action<float, float> OnEnemyShieldDamaged; // Should be shieldHealth and shieldMaxHealth
    public static event Action<float, float> OnEnemyShieldRepaired;

    protected string shipName;

    public virtual Sprite GetHealthSprite => healthDisplaySprite;
    public virtual string GetName => shipName;
    public virtual float GetShipHealth => health;
    public virtual float GetShipMaxHealth => maxHealth;
    public virtual float GetShieldHealth => shieldHealth;
    public virtual float GetShieldMaxHealth => shieldMaxHealth;

    [Header("Victory Sequence")]
    [SerializeField, Min(0f)] private float defeatSequenceDuration = 3f;
    [SerializeField, Min(0f)] private float stagedExplosionDelay = 1f;
    [SerializeField, Min(0f)] private float finalExplosionLeadTime = 0.35f;
    [SerializeField] private UnityEvent defeatStartedEffects;
    [SerializeField] private UnityEvent stagedExplosionEffects;
    [SerializeField] private UnityEvent finalExplosionEffects;

    private bool isDefeated;

    public bool IsDefeated => isDefeated;
    public static event Action<EnemyShip> OnEnemyShipDefeated;
    public static event Action<EnemyShip> OnEnemyShipStagedExplosion;
    public static event Action<EnemyShip> OnEnemyShipFinalExplosion;

    protected virtual void Awake() {
        SetName();
        AssignHealthDisplaySprite();
        SetAsActiveShip();
        GenerateRandomStations();
    }

    protected virtual void GenerateRandomStations()
    {
        if (stationSlots == null || stationSlots.Count == 0) return;
        if (availableStationPrefabs == null || availableStationPrefabs.Count == 0)
        {
            Debug.LogWarning($"[EnemyShip] No station prefabs available to spawn on {gameObject.name}!");
            return;
        }

        // Keep a temporary copy of the pool to prevent duplicate stations on a single ship
        List<EnemyShipStation> poolCopy = new List<EnemyShipStation>(availableStationPrefabs);

        foreach (Transform slot in stationSlots)
        {
            if (slot == null) continue;
            if (poolCopy.Count == 0) break; // Stop if we run out of unique stations

            // 1. Roll a random index from our remaining pool copy
            int randomIndex = UnityEngine.Random.Range(0, poolCopy.Count);
            EnemyShipStation chosenPrefab = poolCopy[randomIndex];

            // 2. Instantiate the prefab as a direct child of the slot transform
            EnemyShipStation newStation = Instantiate(chosenPrefab, slot);

            // 3. Reset transform properties so it aligns perfectly with the slot's UI position
            newStation.transform.localPosition = Vector3.zero;
            newStation.transform.localRotation = Quaternion.identity;
            newStation.transform.localScale = Vector3.one;

            spawnedStations.Add(newStation);

            // Prevents any duplicate stations spawning
            poolCopy.RemoveAt(randomIndex); 
        }
    }

    protected virtual void OnDestroy()
    {
        if (RunManager.Instance != null && RunManager.Instance.activeEnemyShip == this)
        {
            RunManager.Instance.activeEnemyShip = null;
        }
    }

    private void Start()
    {
        OnEnemyShipSpawn?.Invoke(this);
    }

    protected virtual void SetName()
    {
        shipName = "Unnamed"; // Should be overrided in subclasses
    }

    protected virtual void AssignHealthDisplaySprite()
    {
        Transform childTransform = transform.Find("ShipSprite");
        
        if (childTransform != null)
        {
            Image uiImage = childTransform.GetComponent<Image>();
            
            if (uiImage != null)
            {
                healthDisplaySprite = uiImage.sprite;
            }
        }
        else
        {
            Debug.LogError($"[EnemyShip] Hierarchy Error: Could not find any UI Image component named 'ShipSprite' in {gameObject.name}!");
        }
    }

    protected void SetAsActiveShip()
    {
        RunManager.Instance.activeEnemyShip = this;
    }
    public void setShieldStationStatus(bool isStationOnline)
    {
        // Called by the shield ship station if it is attached to the ship.
        hasAShieldStation = isStationOnline;
    }

    public virtual void RepairDamage(float healthRestored) {
        if(isDefeated) 
        {
            return;
        }

        health = Mathf.Min(health + Mathf.Max(0f, healthRestored), maxHealth);
        OnEnemyShipHPChange?.Invoke();
    }

    public virtual void TakeDamage(float damage) {
        if(isDefeated || damage <= 0f) 
        {
            return;
        }

        Debug.Log($"HP before damage: {health}");

        if(shieldHealth > 0f && hasAShieldStation)
        {
            float shieldDamage = Mathf.Min(damage, shieldHealth);
            shieldHealth -= shieldDamage;
            damage -= shieldDamage;
            OnEnemyShieldDamaged?.Invoke(shieldHealth, shieldMaxHealth);

            if(shieldHealth <= 0f)
            {
                shieldHealth = 0f;
                OnEnemyShieldBreak?.Invoke();
            }
        }

        health = Mathf.Max(0f, health - damage);
        bool lethal = health <= 0f;

        if(lethal) 
        {
            isDefeated = true;
        }

        Debug.Log($"HP after damage: {health}");
        OnEnemyShipHPChange?.Invoke();

        if(lethal)
        {
            BeginDefeatSequence();
        }
    }

    private void BeginDefeatSequence()
    {
        Debug.Log("[EnemyShip] Defeat sequence started.", this);
        defeatStartedEffects?.Invoke();
        OnEnemyShipDefeated?.Invoke(this);
        StartCoroutine(DefeatSequence());
    }

    private IEnumerator DefeatSequence()
    {
        float totalDuration = Mathf.Max(0f, defeatSequenceDuration);
        float stagedTime = Mathf.Clamp(stagedExplosionDelay, 0f, totalDuration);

        float finalTime = Mathf.Clamp(totalDuration - finalExplosionLeadTime, stagedTime, totalDuration);

        if (stagedTime > 0f)
        {
            yield return new WaitForSecondsRealtime(stagedTime);
        }

        stagedExplosionEffects?.Invoke();
        OnEnemyShipStagedExplosion?.Invoke(this);

        if (finalTime > stagedTime)
        {
            yield return new WaitForSecondsRealtime(finalTime - stagedTime);
        }

        finalExplosionEffects?.Invoke();
        OnEnemyShipFinalExplosion?.Invoke(this);

        if (totalDuration > finalTime)
        {
            yield return new WaitForSecondsRealtime(totalDuration - finalTime);
        }

        Die();
    }

    protected virtual void Die()
    {
        Debug.Log("[EnemyShip] Death flow beginning.", this);

        if(RunManager.Instance != null && RunManager.Instance.activeEnemyShip == this) 
        {
            RunManager.Instance.activeEnemyShip = null;
        }

        OnEnemyShipDeath?.Invoke(this);
        OnEnemyShipSpawn?.Invoke(null);
        Destroy(gameObject);
    }

    public void RestoreShield()
    {
        if (!hasAShieldStation) return;
        
        shieldHealth = shieldMaxHealth;
        // Trigger the event so the station gradient and any other health bars update
        OnEnemyShieldRepaired?.Invoke(shieldHealth, shieldMaxHealth); 
    }
}
