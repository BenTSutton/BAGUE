using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyShipHPUI : MonoBehaviour
{
    protected EnemyShip enemyShip;
    
    [SerializeField] private GameObject uiContainer;
    
    [Header("HP Overlay Settings")]
    [SerializeField] private Image EnemyShipHealthBackground;
    [SerializeField] private Image EnemyShipDamageIcon;

    [Header("Shield Overlay Settings")]
    [SerializeField] private Image ShieldVisualOverlay;
    [SerializeField] private float shieldAlpha = 0.3f;

    [Header("Shield Recharge UI Settings")]
    [SerializeField] private Slider rechargeSlider; 
    [SerializeField] private float rechargeTime = 5f;

    private Coroutine rechargeCoroutine;
    
    void OnEnable()
    {
        EnemyShip.OnEnemyShipSpawn += HandleShipSpawn;

        ToggleUI(false);

        if (RunManager.Instance != null && RunManager.Instance.activeEnemyShip != null)
        {
            HandleShipSpawn(RunManager.Instance.activeEnemyShip);
        }
        else
        {
            ToggleUI(false); 
        }
    }

    void OnDisable()
    {
        EnemyShip.OnEnemyShipSpawn -= HandleShipSpawn;
        
        if (enemyShip != null)
        {
            enemyShip.OnEnemyShipHPChange -= UpdateEnemyShipHPUI;
            EnemyShip.OnEnemyShieldDamaged -= UpdateEnemyShipShieldUI;
            EnemyShip.OnEnemyShieldRepaired -= UpdateEnemyShipShieldUI;
        }
        ToggleUI(false);
    }

    void HandleShipSpawn(EnemyShip newShip)
    {
        CleanupShipSubscriptions();
        enemyShip = newShip;

        if (enemyShip != null)
        {
            enemyShip.OnEnemyShipHPChange += UpdateEnemyShipHPUI;
            EnemyShip.OnEnemyShieldDamaged += UpdateEnemyShipShieldUI;
            EnemyShip.OnEnemyShieldRepaired += UpdateEnemyShipShieldUI;

            SetHealthDisplaySprite(enemyShip.GetHealthSprite);
            ToggleUI(true);
            UpdateEnemyShipHPUI();

            if (rechargeSlider != null) rechargeSlider.gameObject.SetActive(false);

            if (enemyShip.hasAShieldStation) 
            {
                UpdateEnemyShipShieldUI(enemyShip.GetShieldHealth, enemyShip.GetShieldMaxHealth);
            }
            else
            {
                ShieldVisualOverlay.enabled = false; 
            } 
        }
        else
        {
            ToggleUI(false);
        }
    }

    private void CleanupShipSubscriptions()
    {
        if (rechargeCoroutine != null)
        {
            StopCoroutine(rechargeCoroutine);
            rechargeCoroutine = null;
        }

        if (enemyShip != null)
        {
            enemyShip.OnEnemyShipHPChange -= UpdateEnemyShipHPUI;
            EnemyShip.OnEnemyShieldDamaged -= UpdateEnemyShipShieldUI;
        }
    }

    void SetHealthDisplaySprite (Sprite newSprite)
    {
        if (EnemyShipHealthBackground != null && EnemyShipDamageIcon != null && newSprite != null)
        {
            if (EnemyShipHealthBackground != null) EnemyShipHealthBackground.sprite = newSprite;
            if (EnemyShipDamageIcon != null) EnemyShipDamageIcon.sprite = newSprite;
        }
    }

    void UpdateEnemyShipHPUI()
    {
        float currentHP = enemyShip.GetShipHealth;
        float maxHP = enemyShip.GetShipMaxHealth;
        float healthPercentage = currentHP / maxHP;
        EnemyShipDamageIcon.fillAmount = 1 - healthPercentage;
    }

    void UpdateEnemyShipShieldUI(float shieldHealth, float shieldMaxHealth)
    {
        if (shieldHealth < shieldMaxHealth)
        {
            StartShieldRechargeProcess();
        }

        if (ShieldVisualOverlay == null) return;

        if (shieldHealth <= 0)
        {
            ShieldVisualOverlay.enabled = false; 
            return;
        }
        
        ShieldVisualOverlay.enabled = true;

        ShieldVisualOverlay.UpdateShieldColour(shieldHealth, shieldMaxHealth, shieldAlpha);
    }

    private void StartShieldRechargeProcess()
    {
        if (gameObject.activeInHierarchy && enemyShip.hasAShieldStation)
        {
            if (rechargeCoroutine != null) StopCoroutine(rechargeCoroutine);
            rechargeCoroutine = StartCoroutine(RechargeShieldRoutine());
        }
    }

    private IEnumerator RechargeShieldRoutine()
    {
        if (rechargeSlider != null)
        {
            rechargeSlider.gameObject.SetActive(true);
            rechargeSlider.value = 0f; 
        }

        float elapsedTime = 0f;

        while (elapsedTime < rechargeTime && enemyShip.hasAShieldStation)
        {
            elapsedTime += Time.deltaTime;
            if (rechargeSlider != null)
            {
                rechargeSlider.value = elapsedTime / rechargeTime;
            }
            yield return null;
        }
        
        if (rechargeSlider != null)
        {
            rechargeSlider.gameObject.SetActive(false);
        }

        if (enemyShip != null && enemyShip.hasAShieldStation)
        {
            enemyShip.RestoreShield();
        }

        rechargeCoroutine = null;
    }

    private void ToggleUI(bool isVisible)
    {
        if (uiContainer != null)
        {
            uiContainer.SetActive(isVisible);
        }
    }
}