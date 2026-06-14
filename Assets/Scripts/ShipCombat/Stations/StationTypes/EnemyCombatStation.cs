using System;
using UnityEngine;

public class EnemyCombatStation : EnemyShipStation, IChargeableStation
{
    [SerializeField] protected int weaponDamage;
    
    [Header("Weapon Recharge Values")]
    [SerializeField] protected float weaponCurrentCharge = 0;
    [SerializeField] protected float weaponMaxChargeValue;
    [SerializeField] protected float weaponRechargeRate;

    public event Action<float, float> OnChargeChanged;

    protected bool weaponCharged = false;

    public void FixedUpdate()
    {
        if (!weaponCharged && !stationIsBroken)
        {
            weaponCurrentCharge += weaponRechargeRate;
            // Debug.Log($"[EnemyCombatStation] {weaponCurrentCharge}");
            if(weaponCurrentCharge >= weaponMaxChargeValue)
            {
                weaponCurrentCharge = weaponMaxChargeValue;
                weaponCharged = true;
                
                Debug.Log("[EnemyCombatStation] Weapon is charged");

                ActivateCombatStation();
            }
            OnChargeChanged?.Invoke(weaponCurrentCharge, weaponMaxChargeValue);
        }
    }

    private void ActivateCombatStation()
    {
        if (weaponCharged)
        {
            DamagePlayerShip(weaponDamage);
            Debug.Log("[EnemyCombatStation] Firing Weapon");
            weaponCurrentCharge = 0;
            weaponCharged = false;

            OnChargeChanged?.Invoke(weaponCurrentCharge, weaponMaxChargeValue);
        }
        else
        {
            Debug.Log("[EnemyCombatStation] Weapon is not charged");
        }
    }

    private void DamagePlayerShip(int damage)
    {
        RunManager.Instance.DamageShip(damage);
    }
}
