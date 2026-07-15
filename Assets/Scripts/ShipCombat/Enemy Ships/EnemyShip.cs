using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class EnemyShip : MonoBehaviour
{
    [Header("HP Settings")]
    [SerializeField] protected float health;
    [SerializeField] protected float maxHealth;

    [Header("Shield Settings (Only applied if you have a shield station)")]
    [SerializeField] protected float shieldHealth;
    [SerializeField] protected float shieldMaxHealth;

    [Header("Random Station Selection Elements")]
    [Tooltip("The empty UI/Transform slot objects are pre-placed on your ship sprite to determine where stations will be generated.")]
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
    public static event Action<float, float> OnEnemyShieldDamaged; // Should be given shieldHealth and shieldMaxHealth
    public static event Action<float, float> OnEnemyShieldRepaired;

    protected string shipName;

    protected EnemyShipArchetype currentArchetype;

    public virtual Sprite GetHealthSprite => healthDisplaySprite;
    public virtual string GetName => shipName;
    public virtual float GetShipHealth => health;
    public virtual float GetShipMaxHealth => maxHealth;
    public virtual float GetShieldHealth => shieldHealth;
    public virtual float GetShieldMaxHealth => shieldMaxHealth;

    protected virtual void Awake() {
        SetName();
        AssignHealthDisplaySprite();
        SetAsActiveShip();
    }

    public virtual void ApplyArchetype(EnemyShipArchetype archetype)
    {
        if (archetype == null)
        {
            Debug.LogError($"[EnemyShip] Initialized with null archetype on {gameObject.name}!");
            return;
        }

        currentArchetype = archetype;
        shipName += $" {archetype.archetypeName}"; // Overwrite the display name to show archetype

        // Apply stats from the archetype to your runtime variables
        this.maxHealth = archetype.maxHealth;
        this.health = maxHealth;

        this.shieldMaxHealth = archetype.maxShields;
        this.shieldHealth = shieldMaxHealth;

        GenerateRandomStations();

        OnEnemyShipSpawn?.Invoke(this);
    }

    protected virtual void GenerateRandomStations()
    {
        // update this function to add set stations based on archetype
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

            // Pick a random index from out poolCopy
            int randomIndex = UnityEngine.Random.Range(0, poolCopy.Count);
            EnemyShipStation chosenPrefab = poolCopy[randomIndex];

            // Instantiate the prefab chosen as a direct child of the slot
            EnemyShipStation newStation = Instantiate(chosenPrefab, slot);

            // Reset transform properties so it aligns with the slot's UI position
            newStation.transform.localPosition = Vector3.zero;
            newStation.transform.localRotation = Quaternion.identity;
            newStation.transform.localScale = Vector3.one;

            spawnedStations.Add(newStation);

            // Prevents any duplicate stations spawning
            poolCopy.RemoveAt(randomIndex); 
        }
    }

    protected virtual void OnDestroy() {
        if (RunManager.Instance.activeEnemyShip == this)
        {
            RunManager.Instance.activeEnemyShip = null;
        }
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
        health += healthRestored;
        if (health > maxHealth) health = maxHealth;
    }

    public virtual void TakeDamage(float damage) {
        Debug.Log($"HP before damage: {health}");
        if (shieldHealth > 0 && hasAShieldStation)
        {

            float shieldDamage = Mathf.Min(damage, shieldHealth);
            shieldHealth -= shieldDamage;
            damage -= shieldDamage; // Reduce damage value so the leftover damage goes to hull

            OnEnemyShieldDamaged?.Invoke(shieldHealth, shieldMaxHealth);

            if (shieldHealth <= 0)
            {
                shieldHealth = 0;
                OnEnemyShieldBreak?.Invoke();
            }
        }
        health -= damage;
        Debug.Log($"HP after damage: {health}");
        if (health <= 0) Die();
        OnEnemyShipHPChange?.Invoke();
    }

    public void RestoreShield()
    {
        if (!hasAShieldStation) return;
        
        shieldHealth = shieldMaxHealth;
        // Trigger the event so the station gradient and any other health bars update
        OnEnemyShieldRepaired?.Invoke(shieldHealth, shieldMaxHealth); 
    }

    protected virtual void Die()
    {
        Debug.Log("[EnemyShip] Dies");
        if (RunManager.Instance.activeEnemyShip == this) {RunManager.Instance.activeEnemyShip = null;}
        OnEnemyShipDeath?.Invoke(this);

        // Clears out alot of stuff without having to implement new death event listeners
        OnEnemyShipSpawn?.Invoke(null);
        Destroy(gameObject);
    }

    
}
