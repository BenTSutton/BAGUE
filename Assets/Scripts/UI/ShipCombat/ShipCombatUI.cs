using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShipCombatUI : MonoBehaviour
{
    [Header("HP UI Settings")]
    [SerializeField] private Image ShipHealthBackground;
    [SerializeField] private Image ShipDamageIcon;

    [Header("Colour Settings")]
    [SerializeField] private Color shipBackgroundColor = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color cloakColor = new Color(0.5f, 0.8f, 1f, 0.4f);

    [Header("Shield UI Settings")]
    [SerializeField] private GameObject shieldVisuals;
    [SerializeField] private ShieldColourGradient shieldGradient;
    private Image shieldImage;
    
    private void Start()
    {
        UpdateHealthUI();
        
        shieldImage = shieldVisuals.GetComponent<Image>();

        UpdateShieldUI(RunManager.Instance.currentShieldHealth, RunManager.Instance.maxShieldHealth);
    }

    private void OnEnable()
    {
        RunManager.Instance.OnHealthChange += UpdateHealthUI;
        CloakSystem.OnCloakActivated += ToggleStealth;
        CombatManager.OnPlayerShieldDamaged += UpdateShieldUI;
        CombatManager.OnPlayerShieldRepaired += EnableShieldUI;
        CombatManager.OnPlayerShieldBreak += DisableShieldUI;
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

    private void UpdateShieldUI(float shieldHealth, float shieldMaxHealth) 
    {
        shieldImage.UpdateShieldColour(shieldHealth, shieldMaxHealth);
    }

    private void EnableShieldUI(float shieldHealth, float shieldMaxHealth)
    {
        shieldImage.enabled = true;
        UpdateShieldUI(shieldHealth, shieldMaxHealth);
    }

    private void DisableShieldUI()
    {
        shieldImage.enabled = false;
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
