using System;
using UnityEngine;
using UnityEngine.UI;

public class StationRechargeBarUI : MonoBehaviour
{
    [SerializeField] Slider chargeBar;
    [SerializeField] private GameObject warningIndicator;

    private EnemyCombatStation combatStation;
    private void Awake()
    {
        // Find the combat station component nearby
        combatStation = GetComponent<EnemyCombatStation>();
        if (warningIndicator != null)
        {
            warningIndicator.SetActive(false);
        }
    }
    
    private void OnEnable()
    {
        if (combatStation == null)
        {
            return;
        }

        combatStation.OnChargeChanged += UpdateRechargeBar;
        combatStation.OnStationBroken += HideRechargeBar;
        combatStation.WeaponWarningStarted += ShowWarning;
        combatStation.WeaponWarningEnded += HideWarning;
    }

    private void OnDisable()
    {
        if (combatStation == null)
        {
            return;
        }

        combatStation.OnChargeChanged -= UpdateRechargeBar;
        combatStation.OnStationBroken -= HideRechargeBar;
        combatStation.WeaponWarningStarted -= ShowWarning;
        combatStation.WeaponWarningEnded -= HideWarning;
    }

    private void ShowWarning()
    {
        if (warningIndicator != null)
        {
            warningIndicator.SetActive(true);
        }
    }

    private void HideWarning()
    {
        if (warningIndicator != null)
        {
            warningIndicator.SetActive(false);
        }
    }

    private void HideRechargeBar()
    {
        chargeBar.gameObject.SetActive(false);

        HideWarning();
    }

    public void UpdateRechargeBar(float currentCharge, float maxCharge)
    {
        if (chargeBar != null)
        {
            chargeBar.value = Mathf.Clamp01(currentCharge / maxCharge);
        }

        if (currentCharge <= 0f && warningIndicator != null)
        {
            HideWarning();
        }
    }
}