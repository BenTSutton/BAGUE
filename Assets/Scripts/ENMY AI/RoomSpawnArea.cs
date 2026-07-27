using UnityEngine;

public class RoomSpawnArea : MonoBehaviour
{
    [SerializeField] private RoomHealth room;
    [SerializeField] private Transform[] spawnPoints;

    public RoomHealth Room => room;

    public bool CanSpawn =>
        room != null &&
        !room.IsDestroyed &&
        spawnPoints != null &&
        spawnPoints.Length > 0;

    public Transform GetRandomSpawnPoint()
    {
        if (!CanSpawn)
            return null;

        return spawnPoints[
            Random.Range(0, spawnPoints.Length)];
    }

    private void OnValidate()
    {
        if (room == null)
            room = GetComponent<RoomHealth>();
    }
}