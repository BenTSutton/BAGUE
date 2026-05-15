using UnityEngine;
using UnityEngine.UI;

public class EnemyShipHPUI : MonoBehaviour
{
    protected EnemyShip enemyShip;
    [SerializeField] private GameObject ShipHealthBackground;
    [SerializeField] private Image EnemyShipDamageIcon;
    
    void OnEnable()
    {
        EnemyShip.OnEnemyShipSpawn += HandleShipSpawn;

        if (RunManager.Instance != null && RunManager.Instance.activeEnemyShip != null)
        {
            HandleShipSpawn(RunManager.Instance.activeEnemyShip);
        }
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
        
        UpdateEnemyShipHPUI();
    }

    void UpdateEnemyShipHPUI()
    {
        float currentHP = enemyShip.GetShipHealth;
        float maxHP = enemyShip.GetShipMaxHealth;
        float healthPercentage = currentHP/maxHP;
        Debug.Log($"Current HP: {currentHP}");
        Debug.Log($"Max HP: {maxHP}");
        Debug.Log($"Dmg percentage {healthPercentage}");
        EnemyShipDamageIcon.fillAmount = 1 - healthPercentage;
    }
}
