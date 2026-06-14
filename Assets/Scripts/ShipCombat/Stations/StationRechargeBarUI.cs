using UnityEngine;
using UnityEngine.UI;

public class StationRechargeBarUI : MonoBehaviour
{
    [SerializeField] Slider chargeBar;

    private EnemyCombatStation combatStation;
    private void Awake()
    {
        // Find the combat station component nearby
        combatStation = GetComponent<EnemyCombatStation>();
    }
    
    private void OnEnable()
    {
        combatStation.OnChargeChanged += UpdateRechargeBar;
    }
    
    public void UpdateRechargeBar(float currentCharge, float maxCharge)
    {
        // If the weapon is fully charged, hide the bar completely
        if (currentCharge >= maxCharge)
        {
            chargeBar.value = 1;
        }

        if (chargeBar != null)
        {
            // Normalizes the value to a 0-1 float for the slider fill
            chargeBar.value = currentCharge / maxCharge;
        }
    }
}