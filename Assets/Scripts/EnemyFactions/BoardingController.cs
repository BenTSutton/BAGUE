using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardingController : MonoBehaviour
{
    [Header("Wave Definition")]
    [SerializeField] private BoardingEncounterDefinition boardingEncounter;

    [Header("Spawn Rooms")]
    [SerializeField] private List<RoomSpawnArea> rooms = new();

    [Header("Wave Timing")]
    [SerializeField, Min(0f)] private float firstWarningDelay = 8f;
    [SerializeField, Min(0f)] private float warningDuration = 3f;
    [SerializeField, Min(0f)] private float minimumTimeBetweenWaves = 18f;
    [SerializeField, Min(0f)] private float maximumTimeBetweenWaves = 22f;

    [Tooltip("A new warning waits until this many or fewer previous boarders remain.")]
    [SerializeField, Min(0)] private int maximumSurvivorsBeforeNextWave = 0;

    public event Action<float> WaveWarningStarted;
    public event Action<int> WaveArrived;
    public event Action WavesStopped;

    private readonly List<GameObject> activeEnemies = new();

    private Coroutine encounterRoutine;
    private bool futureWavesEnabled;
    private bool warningActive;
    private bool waveInProgress;
    private bool finalWaveRequested;

    public int ActiveEnemyCount
    {
        get
        {
            RemoveDestroyedEnemies();
            return activeEnemies.Count;
        }
    }

    private void OnEnable()
    {
        EnemyLaunchBayStation.LaunchBayReady += HandleLaunchBayReady;
        EnemyLaunchBayStation.FutureBoardingWavesStopped += StopFutureWaves;
        GameManager.CombatResolutionStarted += StopEncounter;
    }

    private void OnDisable()
    {
        EnemyLaunchBayStation.LaunchBayReady -= HandleLaunchBayReady;
        EnemyLaunchBayStation.FutureBoardingWavesStopped -= StopFutureWaves;
        GameManager.CombatResolutionStarted -= StopEncounter;

        StopEncounter();
    }

    private void HandleLaunchBayReady(EnemyLaunchBayStation launchBay)
    {
        if (launchBay == null || !launchBay.CanLaunchWaves)
        {
            return;
        }

        StartWaveSchedule(boardingEncounter);
    }

    private void StartWaveSchedule(BoardingEncounterDefinition encounter)
    {
        StopEncounter();

        if (!IsEncounterValid(encounter))
        {
            Debug.LogWarning("[BoardingController] Cannot start wave schedule. " + "Check the boarding encounter and its enemy definitions.", this);

            return;
        }

        boardingEncounter = encounter;
        futureWavesEnabled = true;
        encounterRoutine = StartCoroutine(RunWaveSchedule(encounter));
    }

    /// Old function
    public void BeginEncounter(BoardingEncounterDefinition encounter)
    {
        StopEncounter();

        if (!IsEncounterValid(encounter))
        {
            Debug.LogWarning("[BoardingController] Cannot begin encounter. " + "The encounter is missing or invalid.", this);

            return;
        }

        encounterRoutine = StartCoroutine(RunSingleImmediateWave(encounter));
    }

    public void StopEncounter()
    {
        bool hadActiveRoutine = encounterRoutine != null || futureWavesEnabled ||
            warningActive || waveInProgress;

        futureWavesEnabled = false;
        warningActive = false;
        waveInProgress = false;

        if (encounterRoutine != null)
        {
            StopCoroutine(encounterRoutine);
            encounterRoutine = null;
        }

        RemoveDestroyedEnemies();

        if (hadActiveRoutine)
        {
            WavesStopped?.Invoke();
        }
    }

    private void StopFutureWaves()
    {
        futureWavesEnabled = false;
        warningActive = false;

        // Once a wave has arrived, allow that wave to finish spawning
        // Otherwise, cancel the process 
        if (encounterRoutine != null && !waveInProgress)
        {
            StopCoroutine(encounterRoutine);
            encounterRoutine = null;
        }

        WavesStopped?.Invoke();
    }

    private IEnumerator RunWaveSchedule(BoardingEncounterDefinition encounter)
    {
        yield return new WaitForSeconds(firstWarningDelay);

        while (futureWavesEnabled)
        {
            yield return WaitForPreviousWaveToBeControlled();

            if (!futureWavesEnabled)
            {
                break;
            }

            warningActive = true;
            float effectiveWarningDuration =
                GetEffectiveWarningDuration();

            WaveWarningStarted?.Invoke(effectiveWarningDuration);

            yield return new WaitForSeconds(effectiveWarningDuration);

            warningActive = false;

            if (!futureWavesEnabled)
            {
                break;
            }

            waveInProgress = true;
            WaveArrived?.Invoke(encounter.totalEnemies);

            yield return SpawnWave(encounter);

            waveInProgress = false;

            if (!futureWavesEnabled)
            {
                break;
            }

            yield return new WaitForSeconds(GetBetweenWaveDelay());
        }

        encounterRoutine = null;
    }

    private IEnumerator RunSingleImmediateWave(BoardingEncounterDefinition encounter)
    {
        waveInProgress = true;
        WaveArrived?.Invoke(encounter.totalEnemies);

        yield return SpawnWave(encounter);

        waveInProgress = false;
        encounterRoutine = null;
    }

    private IEnumerator WaitForPreviousWaveToBeControlled()
    {
        while (futureWavesEnabled)
        {
            RemoveDestroyedEnemies();

            if (activeEnemies.Count <= maximumSurvivorsBeforeNextWave)
            {
                yield break;
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    private IEnumerator SpawnWave(BoardingEncounterDefinition encounter)
    {
        int enemiesSpawned = 0;

        while (enemiesSpawned < encounter.totalEnemies)
        {
            RemoveDestroyedEnemies();

            if (activeEnemies.Count >= encounter.maximumAliveEnemies)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            if (!TrySpawnRandomEnemy(encounter, enemiesSpawned == 0))
            {
                Debug.LogWarning("[BoardingController] Wave stopped because no enemy " + "could be spawned. Check rooms and enemy definitions.", this);

                yield break;
            }

            enemiesSpawned++;

            if (enemiesSpawned < encounter.totalEnemies)
            {
                yield return new WaitForSeconds(GetSpawnInterval(encounter));
            }
        }
    }

    private bool TrySpawnRandomEnemy(
        BoardingEncounterDefinition encounter,
        bool playArrivalSound)
    {
        List<RoomSpawnArea> availableRooms = new();

        foreach (RoomSpawnArea room in rooms)
        {
            if (room != null &&
                room.gameObject.activeInHierarchy &&
                room.CanSpawn)
            {
                availableRooms.Add(room);
            }
        }

        if (availableRooms.Count == 0)
        {
            return false;
        }

        if (encounter.enemyTypes == null ||
            encounter.enemyTypes.Count == 0)
        {
            return false;
        }

        RoomSpawnArea selectedRoom = availableRooms[UnityEngine.Random.Range(0, availableRooms.Count)];

        EnemyDefinition enemyDefinition = encounter.enemyTypes[UnityEngine.Random.Range(0, encounter.enemyTypes.Count)];

        if (selectedRoom == null || enemyDefinition == null || enemyDefinition.prefab == null)
        {
            return false;
        }

        Transform spawnPoint = selectedRoom.GetRandomSpawnPoint();

        if (spawnPoint == null)
        {
            Debug.LogWarning("[BoardingController] Selected room returned no spawn point.", selectedRoom);

            return false;
        }

        GameObject enemyInstance = Instantiate(enemyDefinition.prefab, spawnPoint.position, spawnPoint.rotation);

        EnemyAI enemyAI = enemyInstance.GetComponent<EnemyAI>();

        if (enemyAI != null)
        {
            RoomHealth targetRoom = selectedRoom.Room;
            enemyAI.Initialize(enemyDefinition, targetRoom);
        }
        else
        {
            Debug.LogWarning("[BoardingController] Spawned enemy has no EnemyAI component.", enemyInstance);
        }

        activeEnemies.Add(enemyInstance);

        // One arrival sound per wave is enough; playing it for every spawned
        // boarder quickly becomes overwhelming.
        if (playArrivalSound)
            SFXManager.Instance?.PlayBoarderAppears(spawnPoint.position);

        return true;
    }

    private void RemoveDestroyedEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    private bool IsEncounterValid(BoardingEncounterDefinition encounter)
    {
        return encounter != null &&
               encounter.totalEnemies > 0 &&
               encounter.maximumAliveEnemies > 0 &&
               encounter.enemyTypes != null &&
               encounter.enemyTypes.Count > 0;
    }

    private float GetBetweenWaveDelay()
    {
        float minimum = Mathf.Min(minimumTimeBetweenWaves, maximumTimeBetweenWaves);

        float maximum = Mathf.Max(minimumTimeBetweenWaves, maximumTimeBetweenWaves);

        return UnityEngine.Random.Range(minimum, maximum);
    }

    private float GetEffectiveWarningDuration()
    {
        if (RunManager.Instance != null &&
            !RunManager.Instance.IsRoomOperational<DeckRoom>())
        {
            return Mathf.Max(0.5f, warningDuration - 1f);
        }

        return warningDuration;
    }

    private static float GetSpawnInterval(BoardingEncounterDefinition encounter)
    {
        float minimum = Mathf.Min(encounter.minimumSpawnInterval, encounter.maximumSpawnInterval);

        float maximum = Mathf.Max(encounter.minimumSpawnInterval, encounter.maximumSpawnInterval);

        return UnityEngine.Random.Range(minimum, maximum);
    }

    public bool RequestFinalWave()
    {
        if (finalWaveRequested)
        {
            return false;
        }

        if (!IsEncounterValid(boardingEncounter))
        {
            Debug.LogWarning("[BoardingController] Final wave skipped because " + "the boarding encounter is invalid.", this);

            return false;
        }

        if (GameManager.Instance == null || GameManager.Instance.IsCombatEnding)
        {
            return false;
        }

        StopEncounter();

        finalWaveRequested = true;
        encounterRoutine = StartCoroutine(RunFinalWarnedWave(boardingEncounter));

        return true;
    }

    private IEnumerator RunFinalWarnedWave(BoardingEncounterDefinition encounter)
    {
        warningActive = true;
        float effectiveWarningDuration =
            GetEffectiveWarningDuration();

        WaveWarningStarted?.Invoke(effectiveWarningDuration);

        yield return new WaitForSeconds(effectiveWarningDuration);

        warningActive = false;
        waveInProgress = true;

        WaveArrived?.Invoke(encounter.totalEnemies);

        yield return SpawnWave(encounter);

        waveInProgress = false;
        encounterRoutine = null;
    }

    public void ConfigureEncounter(BoardingEncounterDefinition encounter)
    {
        if (!IsEncounterValid(encounter))
        {
            Debug.LogWarning("[BoardingController] Invalid boarding encounter.", this);

            return;
        }

        if (encounterRoutine != null)
        {
            StopEncounter();
        }

        boardingEncounter = encounter;
    }
}
