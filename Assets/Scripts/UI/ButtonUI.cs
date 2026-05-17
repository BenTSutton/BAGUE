using System;
using UnityEngine;

public class ButtonUI : MonoBehaviour
{
    [SerializeField] private EnemyShipSpawner shipSpawner;
    [SerializeField] private EnemyFactionProfile newFaction;
    public void DamageShipButton()
    {
        Debug.Log("Damage Button clicked");
        RunManager.Instance.DamageShip(10);
    }
    public void HealShipButton()
    {
        Debug.Log("Damage Button clicked");
        RunManager.Instance.AddHealth(10);
    }

    public void TestClick()
    {
        Debug.Log("Was clicked");
    }

    public void SetActiveFaction()
    {
        shipSpawner.SetActiveFaction(newFaction);
        shipSpawner.SpawnEnemyShip();
    }
}
