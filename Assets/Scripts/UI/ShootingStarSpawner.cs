using System.Collections;
using UnityEngine;

public class ShootingStarSpawner : MonoBehaviour
{
    public ShootingStar starPrefab;

    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -5f;
    public float maxY = 5f;

    public float minInterval = 2f;
    public float maxInterval = 8f;
    public float minSpeed = 10f;
    public float maxSpeed = 20f;

    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Random.Range(minInterval, maxInterval));

            Vector3 position = new(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                transform.position.z);

            Vector2 direction =
                Random.insideUnitCircle.normalized;

            ShootingStar star = Instantiate(
                starPrefab, position,
                Quaternion.identity, transform);

            star.Initialise(
                direction,
                Random.Range(minSpeed, maxSpeed));
        }
    }
}