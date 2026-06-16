using UnityEngine;
using UnityEngine.UI;

public class EnemyShipSpawner : MonoBehaviour
{
    private EnemyFactionProfile activeFactionData;
    public EnemyFactionProfile ActiveFactionData => activeFactionData;

    public void SetActiveFaction(EnemyFactionProfile newEnemy)
    {
        if (newEnemy == null)
        {
            Debug.LogWarning($"[EnemyShipSpawner] Tried to assign a null faction on {gameObject.name}!");
            return;
        }
        activeFactionData = newEnemy;
    }
    public void SpawnEnemyShip()
    {
        if (activeFactionData == null || activeFactionData.EnemyShipPrefab == null)
        {
            Debug.LogError($"[EnemyShipSpawner] No Faction Data or Prefab assigned on {gameObject.name}!");
            return;
        }

        GameObject spawnedShip = Instantiate(activeFactionData.EnemyShipPrefab, transform);

        UpdateShipImageBasedOnType(spawnedShip);

        // Reset the UI layout positions relative to this object
        RectTransform rectTransform = spawnedShip.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Sets the position to 0, 0, 0
            rectTransform.localPosition = Vector3.zero;
        }
        
    }

    private void UpdateShipImageBasedOnType(GameObject ship)
    {
        CombatType combatType = GameManager.Instance.currentCombatNode.combatType;
        Color color = new Color (0f, 0f, 0f);
        switch(combatType)
        {
            case CombatType.Aggressive:
                color = Color.yellow;
                break;
            case CombatType.Tank:
                color = Color.blue;
                break;
        }

        ship.transform.Find("ShipSprite").GetComponent<Image>().color = color;
    }
}
