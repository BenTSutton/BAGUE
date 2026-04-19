using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    private bool hasSpawned = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasSpawned)
        {
            SpawnEnemy();
            hasSpawned = true;
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;

        int index = Random.Range(0, spawnPoints.Length);

        Instantiate(
            enemyPrefab,
            spawnPoints[index].position,
            Quaternion.identity
        );
    }
}