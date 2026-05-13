using System;
using UnityEngine;

public class StationHitByCannon : MonoBehaviour
{
    public static event Action ShotsFired;
    public void DamageEnemyStation(EnemyShipStation station)
    {
        station.DamageShipStation(1); //Currently just dealing 1 damage should be modified to be a specific number, in run manager?
        ShotsFired?.Invoke();
    }
}
