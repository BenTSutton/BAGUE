using System;
using UnityEngine;

public class StationHitByCannon : MonoBehaviour
{
    public static event Action ShotsFired;
    public void DamageEnemyStation(EnemyShipStation station)
    {
        float cannonDamage = 1; //Currently just dealing 1 damage, should be modified to be grabbed from the cannons strength, perhaps defined in run manager?
        station.DamageShipStation(cannonDamage); 
        ShotsFired?.Invoke();
    }
}
