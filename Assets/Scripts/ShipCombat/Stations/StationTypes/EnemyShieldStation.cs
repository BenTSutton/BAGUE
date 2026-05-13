using System;
using UnityEngine;

public class EnemyShieldStation : EnemyShipStation
{
    [SerializeField] private GameObject Shield;

    protected override void Awake()
    {
        // Finds the ship component on this object or any parent
        thisShip = GetComponentInParent<EnemyShip>();
        Shield.SetActive(true);
    }
    private void OnEnable()
    {
        thisShip.OnShieldBreak += DisableShield;
    }

    private void OnDisable()
    {
        thisShip.OnShieldBreak += DisableShield;
    }

    public override void HandleBrokenStation()
    {
        DisableShield();
    }

    private void DisableShield()
    {
        Shield.SetActive(false);
    }
}
