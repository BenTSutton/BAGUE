using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardingController : MonoBehaviour
{
    [SerializeField]
    private List<RoomSpawnArea> rooms = new();

    private readonly List<GameObject> activeEnemies = new();

    private Coroutine encounterRoutine;
    private int enemiesSpawned;

    void Start()
    {
        BeginEncounter(RunManager.Instance.getRandomBoardingEncounter());
    }

    public void BeginEncounter(BoardingEncounterDefinition encounter)
    {
        StopEncounter();

        if (!IsValid(encounter))
            return;

        enemiesSpawned = 0;
        encounterRoutine = StartCoroutine(RunEncounter(encounter));
    }

    public void StopEncounter()
    {
        if (encounterRoutine != null)
        {
            StopCoroutine(encounterRoutine);
            encounterRoutine = null;
        }
    }

    private IEnumerator RunEncounter(BoardingEncounterDefinition encounter)
    {
        while (enemiesSpawned < encounter.totalEnemies)
        {
            RemoveDeadEnemies();

            if (activeEnemies.Count < encounter.maximumAliveEnemies)
            {
                SpawnRandomEnemy(encounter);
            }

            float delay = Random.Range(
                encounter.minimumSpawnInterval,
                encounter.maximumSpawnInterval);

            yield return new WaitForSeconds(delay);
        }

        encounterRoutine = null;
    }

    private void SpawnRandomEnemy(BoardingEncounterDefinition encounter)
    {
        Debug.Log("Spawn Random Enemy");
        RoomSpawnArea room = GetRandomAvailableRoom();

        if (room == null)
        {
            Debug.LogWarning("No undestroyed room has a spawn point.");
            return;
        }

        EnemyDefinition definition = encounter.enemyTypes[Random.Range(0, encounter.enemyTypes.Count)];

        if (definition == null || definition.prefab == null)
        {
            Debug.LogWarning("Encounter contains an invalid enemy definition.");
            return;
        }

        Transform spawnPoint = room.GetRandomSpawnPoint();

        GameObject instance = Instantiate(
            definition.prefab,
            spawnPoint.position,
            Quaternion.identity);

        EnemyAI enemy = instance.GetComponent<EnemyAI>();

        if (enemy == null)
        {
            Debug.LogError(
                $"{definition.prefab.name} has no EnemyAI.",
                instance);

            Destroy(instance);
            return;
        }

        enemy.Initialize(definition, room.Room);

        activeEnemies.Add(instance);
        enemiesSpawned++;
    }

    private RoomSpawnArea GetRandomAvailableRoom()
    {
        List<RoomSpawnArea> available = rooms.FindAll(room => room != null && room.CanSpawn);

        if (available.Count == 0)
            return null;

        return available[Random.Range(0, available.Count)];
    }

    private void RemoveDeadEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    private bool IsValid(BoardingEncounterDefinition encounter)
    {
        if (encounter == null ||
            encounter.totalEnemies <= 0 ||
            encounter.maximumAliveEnemies <= 0 ||
            encounter.enemyTypes == null ||
            encounter.enemyTypes.Count == 0)
        {
            Debug.LogError("Invalid boarding encounter.");
            return false;
        }

        return true;
    }
}