using System.Collections;
using UnityEngine;

public class RatSpawner : MonoBehaviour
{
    [Header("Rat")]
    [SerializeField] private GameObject ratPrefab;

    [Header("Spawn Timing")]
    [SerializeField] private float minSpawnTime = 1f;
    [SerializeField] private float maxSpawnTime = 10f;

    [Header("Direction")]
    [SerializeField] private bool spawnFacingRight = true;

    private void Start()
    {
        StartCoroutine(SpawnRats());
    }

    private IEnumerator SpawnRats()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            GameObject rat = Instantiate(
                ratPrefab,
                transform.position,
                Quaternion.identity
            );

            RatLogic ratLogic = rat.GetComponent<RatLogic>();

            if (ratLogic != null)
            {
                ratLogic.SetDirection(
                    spawnFacingRight ? Vector2.right : Vector2.left
                );
            }
        }
    }
}