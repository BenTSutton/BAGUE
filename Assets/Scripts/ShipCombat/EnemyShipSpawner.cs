using UnityEngine;
using UnityEngine.UI;

public class EnemyShipSpawner : MonoBehaviour
{
    private EnemyFactionProfile activeFactionData;
    public EnemyFactionProfile ActiveFactionData => activeFactionData;
    private GameObject spawnedShip;

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
        if (spawnedShip != null)
        {
            Debug.LogWarning("[EnemyShipSpawner] An enemy ship is already active.", this);

            return;
        }

        if (activeFactionData == null || activeFactionData.EnemyShipPrefab == null)
        {
            Debug.LogError("[EnemyShipSpawner] No faction or enemy ship prefab assigned.", this);

            return;
        }

        spawnedShip = Instantiate(activeFactionData.EnemyShipPrefab, transform);

        UpdateShipImageBasedOnType(spawnedShip);

        RectTransform rectTransform = spawnedShip.GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            rectTransform.localPosition = Vector3.zero;
        }
    }

    private void UpdateShipImageBasedOnType(GameObject ship)
    {
        if (ship == null)
        {
            return;
        }

        CombatDefinition definition = GameManager.Instance != null
            ? GameManager.Instance.currentCombatNode
            : null;

        if (definition == null)
        {
            return;
        }

        Transform shipSprite = ship.transform.Find("ShipSprite");
        Image shipImage = shipSprite != null
            ? shipSprite.GetComponent<Image>()
            : null;

        if (shipImage == null)
        {
            Debug.LogWarning("[EnemyShipSpawner] Spawned ship has no ShipSprite Image.", ship);

            return;
        }

        switch (definition.combatType)
        {
            case CombatType.Aggressive:
                shipImage.color = Color.yellow;
                break;

            case CombatType.Tank:
                shipImage.color = Color.blue;
                break;

            default:
                shipImage.color = Color.white;
                break;
        }
    }
}
