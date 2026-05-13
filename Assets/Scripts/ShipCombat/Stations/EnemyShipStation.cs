using System;
using UnityEngine;

public abstract class EnemyShipStation : MonoBehaviour
{
    [SerializeField] protected float stationHealth;
    [SerializeField] protected float stationMaxHealth;
    protected EnemyShip thisShip;
    public event Action OnStationBroken;

    protected virtual void Awake()
    {
        // Finds the ship component on this object or any parent
        thisShip = GetComponentInParent<EnemyShip>();
    }

    protected bool stationIsBroken = false;
    public virtual void DamageShipStation(float damage)
    {
        Debug.Log($"Attempting to damage station on {thisShip.GetName()}");
        if (stationIsBroken) { Debug.Log("Station already broken");}
        float shieldHealth = thisShip.getShieldHealth();
        thisShip.TakeDamage(damage);
        // Shield losing health is handled in the EnemyShip class
        stationHealth -= damage - shieldHealth;
        Debug.Log("Dealing damage to station");
        if (stationHealth <= 0) { stationHealth = 0; }
        stationIsBroken = stationHealth <= 0;
        
        if (stationIsBroken)
        {
            Debug.Log("Broke the station!");
            ReportStationBroken();
            HandleBrokenStation();
        }   
    }

    public virtual void HandleBrokenStation()
    {
        // Does nothing but can be used in subclasses to have different effects. i.e. shields 
    }

    // This was made a seperate function because events can't be called from any subclasse specific functions but a function with it in the main class can be called.
    protected void ReportStationBroken()
    {
        OnStationBroken?.Invoke();
    }
}
