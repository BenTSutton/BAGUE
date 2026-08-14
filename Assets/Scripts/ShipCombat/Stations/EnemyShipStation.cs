using System;
using UnityEngine;

public abstract class EnemyShipStation : MonoBehaviour
{
    [SerializeField] protected float stationHealth;
    [SerializeField] protected float stationMaxHealth;
    protected EnemyShip enemyShip;
    protected bool stationIsBroken = false;
    public event Action OnStationBroken;
    public bool IsBroken => stationIsBroken;

    public bool CanReceiveCannonShot => enemyShip != null && !enemyShip.IsDefeated && !stationIsBroken;

    public virtual string BrokenMessage =>
        "ENEMY STATION DESTROYED";

    protected virtual void Awake()
    {
        // Finds the ship component on this object or any parent
        enemyShip = GetComponentInParent<EnemyShip>();
    }

    public virtual void DamageShipStation(float damage)
    {
        if(!CanReceiveCannonShot || damage <= 0f) 
        {
            return;
        }

        Debug.Log($"[EnemyShipStation] Health before damage = {stationHealth}");

        if(stationIsBroken) 
        {
            Debug.Log("[EnemyShipStation] Station already broken; hull damage still applies.");
        }

        float shieldHealthBeforeImpact = enemyShip.hasAShieldStation ? enemyShip.GetShieldHealth : 0f;

        enemyShip.TakeDamage(damage);

        if(enemyShip.IsDefeated || stationIsBroken) 
        {
            return;
        }

        float stationDamage = Mathf.Max(0f, damage - shieldHealthBeforeImpact);

        if(stationDamage <= 0f) 
        {
            return;
        }

        stationHealth = Mathf.Max(0f, stationHealth - stationDamage);

        if(stationHealth <= 0f)
        {
            BreakStation();
        }
    }

    private void BreakStation()
    {
        if (stationIsBroken)
            return;

        stationIsBroken = true;
        stationHealth = 0f;

        Debug.Log($"[EnemyShipStation] {name} was destroyed.", this);

        OnStationBroken?.Invoke();
        HandleBrokenStation();
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
