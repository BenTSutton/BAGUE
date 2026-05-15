using System;
using UnityEngine;
using UnityEngine.UI;

public class ShipCombatUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Update is called once per frame
    [SerializeField] private GameObject ShipHealthBackground;
    [SerializeField] private Image ShipDamageIcon;
    
    private void OnEnable()
    {
        RunManager.Instance.OnHealthChange += UpdateHealthUI;
    }

    private void OnDisable()
    {
        RunManager.Instance.OnHealthChange -= UpdateHealthUI;
    }

    private void UpdateHealthUI()
    {
        float currentHP = RunManager.Instance.currentShipHealth;
        float maxHP = RunManager.Instance.maxShipHealth;
        float healthPercentage = currentHP/maxHP;
        Debug.Log($"Current HP: {currentHP}");
        Debug.Log($"Max HP: {maxHP}");
        Debug.Log($"Dmg percentage {healthPercentage}");
        ShipDamageIcon.fillAmount = 1 - healthPercentage;
    }
}
