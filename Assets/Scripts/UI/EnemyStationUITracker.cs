using UnityEngine;

public class EnemyStationUITracker : MonoBehaviour
{
    [Header("UI Grid Setup")]
    [SerializeField] private GameObject blankIconUiPrefab; 

    void OnEnable()
    {
        EnemyShip.OnEnemyShipSpawn += RefreshStationDisplay;
    }

    void OnDisable()
    {
        EnemyShip.OnEnemyShipSpawn -= RefreshStationDisplay;
    }

    private void RefreshStationDisplay(EnemyShip incomingShip)
    {
        // 1. Clear out old layout indicators directly beneath THIS object
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // 2. If the ship died or is null, stop here (the grid will now be empty)
        if (incomingShip == null) return;

        // 3. Find all station UI managers attached down on the physical ship
        EnemyShipStationUI[] stationUIs = incomingShip.GetComponentsInChildren<EnemyShipStationUI>();

        // 4. Generate individual status indicators inside our local layout group
        foreach (EnemyShipStationUI stationUI in stationUIs)
        {
            if (blankIconUiPrefab == null) continue;

            // Spawning with 'transform' sets THIS object as the parent automatically
            GameObject newIconInstance = Instantiate(blankIconUiPrefab, transform);
            
            RectTransform rectTransform = newIconInstance.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localPosition = Vector3.zero;
                rectTransform.localScale = Vector3.one;
            }

            // 5. Attach the event-driven status mirror
            StationStatusMirror mirrorScript = newIconInstance.AddComponent<StationStatusMirror>();
            mirrorScript.InitializeMirror(stationUI);
        }
    }
}
