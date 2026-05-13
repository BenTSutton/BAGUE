using System;
using UnityEngine;

public abstract class EnemyShip : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float shieldHealth;
    [SerializeField] protected float shieldMaxHealth;
    protected Transform player;

    public event Action OnShieldBreak;

    protected string shipName;


    public virtual string GetName()
    {
        return shipName;
    }

    public float getShieldHealth()
    {
        return shieldHealth;
    }

    public virtual void RepairDamage(float healthRestored) {
        health += healthRestored;
        if (health > maxHealth) health = maxHealth;
    }

    public virtual void TakeDamage(float damage) {
        Debug.Log($"HP before damage: {health}");
        if (shieldHealth > 0)
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
