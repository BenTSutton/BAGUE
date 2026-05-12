using System;
using UnityEngine;

public abstract class EnemyShipStation : MonoBehaviour
{
    [SerializeField] protected int stationHealth;
    [SerializeField] protected int stationMaxHealth;
    protected EnemyShip thisShip;
    public event Action OnStationBroken;

    protected virtual void Awake()
    {
        // Finds the ship component on this object or any parent
        thisShip = GetComponentInParent<EnemyShip>();
    }

    protected bool stationIsBroken = false;
    public virtual void DamageShipStation(int damage)
    {
        Debug.Log($"Attempting to damage station on {thisShip.GetName()}");
        if (stationIsBroken) { Debug.Log("Station already broken");}
        stationHealth -= damage;
        Debug.Log("Dealing damage to station");
        if (stationHealth <= 0) { stationHealth = 0; }
        stationIsBroken = stationHealth <= 0;
        
        if (stationIsBroken)
        {
            Debug.Log("Broke the station!");
            OnStationBroken?.Invoke();
        }
        
    }
}
