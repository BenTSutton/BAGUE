using System;
using UnityEngine;

public class StationHitByCannon : MonoBehaviour
{
    public static event Action ShotsFired;
    public void DamageEnemyStation(EnemyShipStation station)
    {
        station.DamageShipStation(1);
        ShotsFired?.Invoke();
    }
}
