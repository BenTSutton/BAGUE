using System;
using UnityEngine;

public class EnemyCombatStation : EnemyShipStation, IChargeableStation
{
    [SerializeField] protected int weaponDamage;
    
    [Header("Weapon Recharge Values")]
    [SerializeField] protected float weaponCurrentCharge = 0;
    [SerializeField] protected float weaponMaxChargeValue;
    [SerializeField] protected float weaponRechargeRate;
    [Header("Warning")]
    [SerializeField, Range(0.1f, 0.99f)]
    private float warningThreshold = 0.75f;

    public event Action<float, float> OnChargeChanged;
    public event Action WeaponWarningStarted;
    public event Action WeaponWarningEnded;

    public static event Action<float> IncomingAttackWarningStarted;
    public static event Action IncomingAttackWarningEnded;
    public static event Action CloakEvadeSucceeded;
    public static event Action PassiveDodgeSucceeded;
    public static event Action<int> EnemyAttackHit;

    protected bool weaponCharged = false;
    private bool warningRaised;
    private float rechargeRateMultiplier = 1f;
    private float EffectiveRechargeRate =>
    weaponRechargeRate * rechargeRateMultiplier;

    public override string BrokenMessage =>
        "WEAPON DESTROYED — ENEMY HULL ATTACKS STOPPED";

    private float TimeUntilWeaponFires
    {
        get
        {
            if (EffectiveRechargeRate <= 0f)
            {
                return 0f;
            }

            return Mathf.Max(0f, (weaponMaxChargeValue - weaponCurrentCharge) / EffectiveRechargeRate);
        }
    }

    private void OnDisable()
    {
        EndWarning();
    }

    private void Update()
    {
        if (stationIsBroken || weaponCharged || !CombatIsRunning())
        {
            return;
        }

        weaponCurrentCharge = Mathf.Min(weaponCurrentCharge + EffectiveRechargeRate  * Time.deltaTime, weaponMaxChargeValue);

        float chargePercentage = weaponCurrentCharge / weaponMaxChargeValue;

        if (!warningRaised && chargePercentage >= warningThreshold)
        {
            BeginWarning();
        }

        OnChargeChanged?.Invoke(weaponCurrentCharge, weaponMaxChargeValue);

        if (weaponCurrentCharge < weaponMaxChargeValue)
            return;

        weaponCharged = true;
        Debug.Log("[EnemyCombatStation] Weapon is charged");

        ActivateCombatStation();
    }

    private void BeginWarning()
    {
        warningRaised = true;

        float remainingTime = TimeUntilWeaponFires;

        Debug.Log($"[EnemyCombatStation] Incoming fire in " +$"{remainingTime:0.0} seconds.", this);

        WeaponWarningStarted?.Invoke();
        IncomingAttackWarningStarted?.Invoke(remainingTime);
    }

    private void ActivateCombatStation()
    {
        if(!weaponCharged || stationIsBroken || !CombatIsRunning() || RunManager.Instance == null)
        {
            ResetWeaponCycle();
            return;
        }

        Debug.Log("[EnemyCombatStation] Resolving enemy shot.", this);

        if(RunManager.Instance.isCloaked)
        {
            CloakEvadeSucceeded?.Invoke();
        }
        else if(RunManager.Instance.CheckIfDodged())
        {
            PassiveDodgeSucceeded?.Invoke();
        }
        else
        {
            RunManager.Instance.DamageShip(weaponDamage);

            if(CameraShake.Instance != null) 
            {
                CameraShake.Instance.TriggerShake(0.2f, 8f);
            }

            EnemyAttackHit?.Invoke(weaponDamage);
        }

        ResetWeaponCycle();
    }

    private void ResetWeaponCycle()
    {
        weaponCurrentCharge = 0f;
        weaponCharged = false;

        EndWarning();

        OnChargeChanged?.Invoke(weaponCurrentCharge, weaponMaxChargeValue);
    }

    private void EndWarning()
    {
        if (!warningRaised)
        {
            return;
        }

        warningRaised = false;

        WeaponWarningEnded?.Invoke();
        IncomingAttackWarningEnded?.Invoke();
    }

    private void DamagePlayerShip(int damage)
    {
        RunManager.Instance.DamageShip(damage);
    }

    public override void HandleBrokenStation()
    {
        ResetWeaponCycle();

        Debug.Log("[EnemyCombatStation] Weapon disabled permanently.", this);
    }

    public void SetRechargeRateMultiplier(float multiplier)
    {
        if (stationIsBroken)
        {
            return;
        }

        rechargeRateMultiplier = Mathf.Max(0f, multiplier);

        Debug.Log($"[EnemyCombatStation] Recharge multiplier set to " + $"{rechargeRateMultiplier:0.##}.", this);
    }

    private bool CombatIsRunning()
    {
        if(GameManager.Instance == null || GameManager.Instance.IsCombatEnding)
        { 
            return false;
        }

        if(enemyShip == null || enemyShip.IsDefeated) 
        {
            return false;
        }

        GameState state = GameManager.Instance.currentState;
        return state == GameState.Combat || state == GameState.Aiming;
    }
}
