using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class EnemyShip : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float shieldHealth;
    [SerializeField] protected float shieldMaxHealth;
    protected Sprite healthDisplaySprite;
    
    // Has a shield is turned off by default and if the ship has a EnemyShieldStation then it will be enabled by that station.
    public bool hasAShieldStation { get; private set; } = false;
    protected Transform player;

    public event Action OnShieldBreak;
    public event Action OnEnemyShipHPChange;
    public static event Action<EnemyShip> OnEnemyShipSpawn;
    public static event Action<EnemyShip> OnEnemyShipDeath;

    protected string shipName;

    public virtual Sprite GetHealthSprite => healthDisplaySprite;
    public virtual string GetName => shipName;
    public virtual float GetShipHealth => health;
    public virtual float GetShipMaxHealth => maxHealth;
    public virtual float GetShieldHealth => shieldHealth;

    protected virtual void Awake() {
        SetName();
        AssignHealthDisplaySprite();
        SetAsActiveShip();
    }

    protected virtual void OnDestroy() {
        if (RunManager.Instance.activeEnemyShip == this)
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
    public void EnableShield()
    {
        // Called by the shield ship station if it is attached to the ship.
        hasAShieldStation = true;
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

            if (shieldHealth <= 0)
            {
                shieldHealth = 0;
                OnShieldBreak?.Invoke();
            }
        }
        health -= damage;
        Debug.Log($"HP after damage: {health}");
        if (health <= 0) Die();
        OnEnemyShipHPChange?.Invoke();
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
