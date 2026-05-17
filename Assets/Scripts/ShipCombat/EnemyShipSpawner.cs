using UnityEngine;

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

        // Reset the UI layout positions relative to this object
        RectTransform rectTransform = spawnedShip.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Sets the position to 0, 0, 0
            rectTransform.localPosition = Vector3.zero;
        }
    }
}
