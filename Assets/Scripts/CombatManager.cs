using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField]
    private EnemyShipSpawner enemyShipSpawner;

    [SerializeField]
    private EnemyFactionProfile enemyFactionProfile;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyShipSpawner.SetActiveFaction(enemyFactionProfile);
        enemyShipSpawner.SpawnEnemyShip();
    }
}
