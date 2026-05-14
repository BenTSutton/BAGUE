using System;
using UnityEngine;

public abstract class EnemyShip : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float shieldHealth;
    [SerializeField] protected float shieldMaxHealth;
    
    // Has a shield is turned off by default and if the ship has a EnemyShieldStation then it will be enabled by that station.
    public bool hasAShieldStation { get; private set; } = false;
    protected Transform player;

    public event Action OnShieldBreak;

    protected string shipName;

    public virtual string GetName => shipName;

    public void EnableShield()
    {
        hasAShieldStation = true;
    }

    public float GetShieldHealth => shieldHealth;

    public virtual void RepairDamage(float healthRestored) {
        health += healthRestored;
        if (health > maxHealth) health = maxHealth;
    }

    public virtual void TakeDamage(float damage) {
        Debug.Log($"HP before damage: {health}");
        if (shieldHealth > 0 & hasAShieldStation)
        {

            float damageAfterShield = damage - shieldHealth;
            shieldHealth -= damage;
            if (shieldHealth <= 0)
            {
                shieldHealth = 0;
                OnShieldBreak?.Invoke();
            }
            damage = damageAfterShield;
        }
        health -= damage;
        Debug.Log($"HP after damage: {health}");
        if (health <= 0) Die();
    }

    protected virtual void Die()
    {
        Debug.Log("Would die");
        throw new NotImplementedException();
    }
}
