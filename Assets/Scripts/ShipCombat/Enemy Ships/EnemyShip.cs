using System;
using UnityEngine;

public abstract class EnemyShip : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float max_health;
    protected Transform player;

    protected string shipName;


    public virtual string GetName()
    {
        return shipName;
    }

    public virtual void RepairDamage(float healthRestored) {
        health += healthRestored;
        if (health > max_health) health = max_health;
    }

    public virtual void TakeDamage(float damage) {
        health -= damage;
        if (health <= 0) Die();
    }

    protected virtual void Die()
    {
        Debug.Log("Would die");
        throw new NotImplementedException();
    }
}
