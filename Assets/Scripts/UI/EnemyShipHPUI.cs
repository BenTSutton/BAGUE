using UnityEngine;
using UnityEngine.UI;

public class EnemyShipHPUI : MonoBehaviour
{
    protected EnemyShip enemyShip;
    [SerializeField] private GameObject uiContainer;
    [SerializeField] private Image ShipHealthBackground;
    [SerializeField] private Image EnemyShipDamageIcon;
    
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
        }
        ToggleUI(false);
    }

    void HandleShipSpawn(EnemyShip newShip)
    {
        // If there is already a ship subscribed to the event unsubscribe from it
        if (enemyShip != null)
        {
            enemyShip.OnEnemyShipHPChange -= UpdateEnemyShipHPUI;
        }

        enemyShip = newShip;

        // Subscribe new ship to UpdateEnemyShipHPUI or if its null disable the health ui.
        if (enemyShip != null)
        {
            enemyShip.OnEnemyShipHPChange += UpdateEnemyShipHPUI;
            SetHealthDisplaySprite(enemyShip.GetHealthSprite);
            UpdateEnemyShipHPUI();
            ToggleUI(true);
        }
        else
        {
            ToggleUI(false);
        }
    }

    void SetHealthDisplaySprite (Sprite newSprite)
    {
        if (ShipHealthBackground != null && EnemyShipDamageIcon != null && newSprite != null)
        {
            ShipHealthBackground.sprite = newSprite;
            EnemyShipDamageIcon.sprite = newSprite;
        }
    }

    void UpdateEnemyShipHPUI()
    {
        float currentHP = enemyShip.GetShipHealth;
        float maxHP = enemyShip.GetShipMaxHealth;
        float healthPercentage = currentHP/maxHP;
        Debug.Log($"Current HP: {currentHP}");
        Debug.Log($"Max HP: {maxHP}");
        Debug.Log($"Health percentage {healthPercentage}");
        EnemyShipDamageIcon.fillAmount = 1 - healthPercentage;
    }

    private void ToggleUI(bool isVisible)
    {
        if (uiContainer != null)
        {
            uiContainer.SetActive(isVisible);
        }
    }
}
