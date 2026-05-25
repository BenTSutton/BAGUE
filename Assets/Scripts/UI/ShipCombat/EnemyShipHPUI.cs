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
    [SerializeField] private Image ShieldVisualOverlay; // <-- Drag your new Shield Image here
    [SerializeField] private ShieldColourGradient shieldGradient;
    [SerializeField] private float shieldAlpha = 0.3f;
    
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
            ToggleUI(false); // Hide immediately if there is no active ship on startup
        }
    }

    void OnDisable()
    {
        EnemyShip.OnEnemyShipSpawn -= HandleShipSpawn;
        
        if (enemyShip != null)
        {
            enemyShip.OnEnemyShipHPChange -= UpdateEnemyShipHPUI;
            EnemyShip.OnEnemyShieldChanged -= UpdateEnemyShipShieldUI;
        }
        ToggleUI(false);
    }

    void HandleShipSpawn(EnemyShip newShip)
    {
        // If there is already a ship subscribed to the event unsubscribe from it
        if (enemyShip != null)
        {
            enemyShip.OnEnemyShipHPChange -= UpdateEnemyShipHPUI;
            EnemyShip.OnEnemyShieldChanged -= UpdateEnemyShipShieldUI;
        }

        enemyShip = newShip;

        // Subscribe new ship to UpdateEnemyShipHPUI UpdateEnemyShipShieldUI and  or if its null disable the health ui.
        if (enemyShip != null)
        {
            enemyShip.OnEnemyShipHPChange += UpdateEnemyShipHPUI;
            EnemyShip.OnEnemyShieldChanged += UpdateEnemyShipShieldUI;

            SetHealthDisplaySprite(enemyShip.GetHealthSprite);
            
            ToggleUI(true);

            UpdateEnemyShipHPUI();

            if (enemyShip.hasAShieldStation) {
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

    void SetHealthDisplaySprite (Sprite newSprite)
    {
        if (EnemyShipHealthBackground != null && EnemyShipDamageIcon != null && newSprite != null)
        {
            if (EnemyShipHealthBackground != null) EnemyShipHealthBackground.sprite = newSprite;
            if (EnemyShipDamageIcon != null) EnemyShipDamageIcon.sprite = newSprite;
            // if (ShieldVisualOverlay != null) ShieldVisualOverlay.sprite = newSprite; this would make it the same shape as the ship.
        }
    }

    void UpdateEnemyShipHPUI()
    {
        float currentHP = enemyShip.GetShipHealth;
        float maxHP = enemyShip.GetShipMaxHealth;
        float healthPercentage = currentHP/maxHP;
        Debug.Log($"[EnemyShipHP UI] Current HP: {currentHP}");
        Debug.Log($"[EnemyShipHP UI] Max HP: {maxHP}");
        Debug.Log($"[EnemyShipHP UI] Health percentage {healthPercentage}");
        EnemyShipDamageIcon.fillAmount = 1 - healthPercentage;
    }

    void UpdateEnemyShipShieldUI(float shieldHealth, float shieldMaxHealth)
    {
        if (ShieldVisualOverlay == null || shieldGradient == null) return;

        if (shieldHealth <= 0)
        {
                ShieldVisualOverlay.enabled = false; 
                return;
        }
        
        ShieldVisualOverlay.enabled = true;

        if (shieldGradient != null)
        {
            Color calculatedColor = shieldGradient.GetColor(shieldHealth, shieldMaxHealth);
            calculatedColor.a = shieldAlpha;
            ShieldVisualOverlay.color = calculatedColor;
        } 
    }

    private void ToggleUI(bool isVisible)
    {
        if (uiContainer != null)
        {
            uiContainer.SetActive(isVisible);
        }
    }
}
