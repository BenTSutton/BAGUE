using UnityEngine;
using UnityEngine.UI;

public class EnemyShipHPUI : MonoBehaviour
{
    protected EnemyShip enemyShip;
    [SerializeField] private Image ShipHealthBackground;
    [SerializeField] private Image EnemyShipDamageIcon;
    
    void OnEnable()
    {
        EnemyShip.OnEnemyShipSpawn += HandleShipSpawn;
    }

    void OnDisable()
    {
        EnemyShip.OnEnemyShipSpawn -= HandleShipSpawn;
        
        if (enemyShip != null)
        {
            enemyShip.OnEnemyShipHPChange -= UpdateEnemyShipHPUI;
        }
    }

    void HandleShipSpawn(EnemyShip newShip)
    {
        // If there is already a ship subscribed to the event unsubscribe it then subscribe the new one.
        if (enemyShip != null)
        {
            enemyShip.OnEnemyShipHPChange -= UpdateEnemyShipHPUI;
        }

        enemyShip = newShip;
        enemyShip.OnEnemyShipHPChange += UpdateEnemyShipHPUI;
        SetHealthDisplaySprite(enemyShip.GetHealthSprite);
        UpdateEnemyShipHPUI();
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
}
