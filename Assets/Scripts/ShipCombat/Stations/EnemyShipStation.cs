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
        float shieldHealth = 0;

        // If the ship has a shield then put its health value into shield health
        if (thisShip.hasAShieldStation) {shieldHealth = thisShip.GetShieldHealth();}
        
        // Shield losing health is handled in the EnemyShip class so can be ignored here
        thisShip.TakeDamage(damage);
        
        stationHealth -= damage - shieldHealth;
        Debug.Log("Dealing damage to station");
        if (stationHealth <= 0) 
        { 
            stationHealth = 0; 
            stationIsBroken = true;
        }
        
        if (stationIsBroken)
        {
            Debug.Log("Broke the station!");
            ReportStationBroken();
            HandleBrokenStation();
        }   
    }

    public virtual void HandleBrokenStation()
    {
        // Does nothing but can be used in subclasses to have different effects. i.e. shields will remove the ui element 
    }

    // This was made a seperate function because events can't be called from any subclasse specific functions but a function with it in the main class can be called.
    protected void ReportStationBroken()
    {
        // Lets the station UI script know that the station is broken and lets it update accordingly
        OnStationBroken?.Invoke();
    }
}
