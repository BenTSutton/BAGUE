using System;
using UnityEngine;

public class EnemyShieldStation : EnemyShipStation
{
    [SerializeField] private GameObject ShieldVisuals;

    protected override void Awake()
    {
        // Finds the ship component on this object or any parent
        thisShip = GetComponentInParent<EnemyShip>();
        ShieldVisuals.SetActive(true);
        thisShip.EnableShield();
    }
    private void OnEnable()
    {
        thisShip.OnShieldBreak += HandleBrokenStation;
    }

    private void OnDisable()
    {
        thisShip.OnShieldBreak += HandleBrokenStation;
    }

    public override void HandleBrokenStation()
    {
        DisableShield();
    }

    private void DisableShield()
    {
        ShieldVisuals.SetActive(false);
    }
}
