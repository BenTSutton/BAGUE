using System;
using UnityEngine;
using UnityEngine.UI;

public class StationStatusMirror : MonoBehaviour
{
    private Image mirrorImage;
    private EnemyShipStationUI targetStationUI;

    private IChargeableStation chargeableStation;
    private Slider localRechargeSlider;

    public void InitializeMirror(EnemyShipStationUI targetUI)
    {
        mirrorImage = GetComponent<Image>();
        targetStationUI = targetUI;

        if (targetStationUI != null && mirrorImage != null)
        {
            // Give this global display icon the exact same graphic profile the station uses
            mirrorImage.sprite = targetStationUI.GetStationSprite;
            
            // Checks if it is a chargeableStation and then if it is displays it
            chargeableStation = targetStationUI.GetComponent<IChargeableStation>();
            localRechargeSlider = GetComponentInChildren<Slider>(true);

            targetStationUI.OnStationColourChanged += SyncMirrorColor;

            if (chargeableStation != null && localRechargeSlider != null)
            {
                localRechargeSlider.gameObject.SetActive(true);
                chargeableStation.OnChargeChanged += UpdateLocalRechargeSlider;
            }
        }
    }

    private void SyncMirrorColor()
    {
         // Copy the color from the station UI
        if (targetStationUI != null && mirrorImage != null)
        {
            mirrorImage.color = targetStationUI.GetComponentInChildren<Image>().color;
        }
    }
    
    private void UpdateLocalRechargeSlider(float currentCharge, float maxCharge)
    {
        localRechargeSlider.value = currentCharge / maxCharge;
    }
}
