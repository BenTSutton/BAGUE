using UnityEngine;

public class StarSpawner : MonoBehaviour
{
    public GameObject starPrefab;
    public int starCount = 20;

    public int rows = 5;
    public int col = 5;

    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -5f;
    public float maxY = 5f;

    void Start()
    {
        SpawnStars();
    }

    void SpawnStars()
    {
        for (int i = 0; i < starCount; i++)
        {
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);

            Vector3 spawnPosition = new Vector3(randomX, randomY, 0f);

            Instantiate(starPrefab, spawnPosition, Quaternion.identity, transform);
        }
    }
} 