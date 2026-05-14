using System;
using UnityEngine;

public class StationHitByCannon : MonoBehaviour
{
    public static event Action ShotsFired;
    public void DamageEnemyStation(EnemyShipStation station)
    {
        // Gets what the currently active cannons damage is from run manager. Cannon script sets the active cannon.
        float cannonDamage = RunManager.Instance.activeCannon.strength; 
        station.DamageShipStation(cannonDamage);
        // Should add some logic here to have it only invoke the shot's fired event if the ship actually fired 
        ShotsFired?.Invoke();
        // Clears the active cannon
        RunManager.Instance.activeCannon = null;
    }
}
