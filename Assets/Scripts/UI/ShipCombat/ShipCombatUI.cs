using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShipCombatUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Update is called once per frame
    [SerializeField] private Image ShipHealthBackground;
    [SerializeField] private Image ShipDamageIcon;

    [SerializeField] private Color shipBackgroundColor = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color cloakColor = new Color(0.5f, 0.8f, 1f, 0.4f);

    [SerializeField] private float debugStealthDuration = 5.0f;
    
    private void OnEnable()
    {
        RunManager.Instance.OnHealthChange += UpdateHealthUI;
        CloakSystem.OnCloakActivated += ToggleStealth;
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
        Debug.Log($"Health percentage {healthPercentage}");
        ShipDamageIcon.fillAmount = 1 - healthPercentage;
    }

    private void ToggleStealth(float lengthOfStealthDuration)
    {
        StartCoroutine(IChangePlayerShipUItoCloakedUI(lengthOfStealthDuration));
    }

    private IEnumerator IChangePlayerShipUItoCloakedUI(float duration)
    {

        ShipHealthBackground.color = cloakColor;
        ShipDamageIcon.color = new Color(ShipDamageIcon.color.r, ShipDamageIcon.color.g, ShipDamageIcon.color.b, 0.4f);

        yield return new WaitForSeconds(duration);

        ShipHealthBackground.color = shipBackgroundColor;
        ShipDamageIcon.color = new Color(ShipDamageIcon.color.r, ShipDamageIcon.color.g, ShipDamageIcon.color.b, 1f);
    }
}
