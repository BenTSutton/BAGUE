using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Menu,
    Navigation,
    Combat,
    Aiming
}

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    public GameState currentState;

    public bool debug;

    public string captainName;
    public string difficulty;

    public CombatDefinition currentCombatNode;

    private bool combatResolutionInProgress;

    [Header("Combat Time")]
    [SerializeField, Range(0.05f, 1f)]
    private float aimingTimeScale = 0.3f;

    private float normalFixedDeltaTime;

    public static event Action CombatResolutionStarted;

    private bool victoryPresentationInProgress;
    public bool IsCombatEnding => combatResolutionInProgress || victoryPresentationInProgress;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        normalFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void OnEnable()
    {
        EnemyShip.OnEnemyShipDeath += HandleEnemyShipDeath;
        RunManager.OnPlayerShipDestroyed += HandlePlayerShipDeath;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnemyShip.OnEnemyShipDefeated += HandleEnemyShipDefeated;
    }

    private void OnDisable()
    {
        EnemyShip.OnEnemyShipDeath -= HandleEnemyShipDeath;
        RunManager.OnPlayerShipDestroyed -= HandlePlayerShipDeath;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        EnemyShip.OnEnemyShipDefeated -= HandleEnemyShipDefeated;

        if (Instance == this)
        {
            RestoreNormalTime();
        }
    }

    public void StartGame(string name, string diff)
    {
        captainName = name;
        difficulty = diff;
        ChangeState(GameState.Navigation);
        gameObject.GetComponent<MapGenerator>().GenerateMap();
    }

    void EnterMap()
    {
        Debug.Log("Should move to map");
        SceneManager.LoadScene(sceneName:"MapScene");
        EnableMapObjects();
        MusicManager.Instance?.PlayMapMusic();
    }

    void EnterCombat()
    {
        Debug.Log("Should enter combat");
        DisableMapObjects();
        SceneManager.LoadScene(sceneName:"NewAlfieCombatScene");
        MusicManager.Instance?.PlayCombatMusic();
    }

    void Update()
    {

        if(Input.GetKeyDown(KeyCode.L) && currentState == GameState.Combat && debug)
        {
            WinCombat();
        }
    }

    void HandleEnemyShipDeath(EnemyShip defeatedShip)
    {
        Debug.Log($"[GameManager] Handeling the defeat of {defeatedShip.GetName}");
        WinCombat();
    }

    private void HandlePlayerShipDeath()
    {
        Debug.Log($"[GameManager] Handeling player defeat");
        LoseCombat();
    }

    

    void WinCombat()
    {
        if(combatResolutionInProgress) 
        {
            return;
        }

        bool presentationAlreadyStarted = victoryPresentationInProgress;
        victoryPresentationInProgress = false;
        combatResolutionInProgress = true;

        if(!presentationAlreadyStarted) 
        {
            CombatResolutionStarted?.Invoke();
        }

        RestoreNormalTime();
        StartCoroutine(WinCombatRoutine());
    }

    public void LoseCombat()
    {
        if (IsCombatEnding)
            return;

        combatResolutionInProgress = true;
        CombatResolutionStarted?.Invoke();
        RestoreNormalTime();
        StartCoroutine(LoseCombatRoutine());
    }

    IEnumerator LoseCombatRoutine()
    {
        ChangeState(GameState.Navigation);

        while (SceneManager.GetActiveScene().name != "MapScene")
        {
            yield return null;
        }

        // Wait one extra frame to allow for awake and start
        yield return null;

        RunManager.Instance.LoseGame();
    }

    IEnumerator WinCombatRoutine()
    {
        RunManager.Instance.ApplyPostCombatRoomEffects();
        ChangeState(GameState.Navigation);

        while (SceneManager.GetActiveScene().name != "MapScene")
        {
            yield return null;
        }

        // Wait one extra frame to allow for awake and start
        yield return null;

        if (RunManager.Instance.inBossFight)
        {
            RunManager.Instance.CompleteBossFight(true);
        }
        else
        {
            MapRunState.Instance.CompleteCombatNode(currentCombatNode);
        }
    }

    // public void LoseCombat()
    // {
    //     ChangeState(GameState.Navigation);
    //     RunManager.Instance.LoseGame();
    // }

    public void ChangeState(GameState newState)
    {
        GameState oldState = currentState;
        currentState = newState;

        ApplyTimeScaleForState(newState);

        switch (newState)
        {
            case GameState.Navigation:
                EnterMap();
                break;
            case GameState.Combat:
                Debug.Log("Switched to combat state");
                if (oldState != GameState.Aiming)
                {
                    RunManager.Instance.BeginCombatRoomEffects();
                    EnterCombat();
                }
                break;
            case GameState.Aiming:
                Debug.Log("Switched to aiming state");
                break;
        }
    }

    public void EnterCombat(CombatDefinition combatDefinition)
    {
        currentCombatNode = combatDefinition;
        combatResolutionInProgress = false;
        victoryPresentationInProgress = false;
        ChangeState(GameState.Combat);
    }

    void DisableMapObjects()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    void EnableMapObjects()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void FinishGame()
    {
        RestoreNormalTime();
        SceneManager.LoadScene(sceneName:"Menu");
        ResetGame();
    }

    void ResetGame()
    {
        captainName = "";
        difficulty = "";
        RunManager.Instance.Reset();
        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // A new scene must never be slow!
        RestoreNormalTime();
    }

    private void ApplyTimeScaleForState(GameState state)
    {
        CombatFeedback.Instance?.CancelHitStop();

        float targetTimeScale = state == GameState.Aiming
                ? Mathf.Clamp(aimingTimeScale, 0.05f, 1f)
                : 1f;

        Time.timeScale = targetTimeScale;
        Time.fixedDeltaTime = normalFixedDeltaTime * targetTimeScale;
    }

    public void RestoreNormalTime()
    {
        CombatFeedback.Instance?.CancelHitStop();

        Time.timeScale = 1f;
        Time.fixedDeltaTime = normalFixedDeltaTime;
    }

    public void ResolveEnemyEscape()
    {
        if (IsCombatEnding)
            return;

        combatResolutionInProgress = true;
        CombatResolutionStarted?.Invoke();
        RestoreNormalTime();

        StartCoroutine(EnemyEscapeRoutine());
    }

    private IEnumerator EnemyEscapeRoutine()
    {
        ChangeState(GameState.Navigation);

        while (SceneManager.GetActiveScene().name != "MapScene")
        {
            yield return null;
        }

        yield return null;

        if (MapRunState.Instance != null)
        {
            MapRunState.Instance.CompleteCombatNodeWithoutRewards("The enemy escaped. No salvage was recovered.");
        }
    }

    private void HandleEnemyShipDefeated(EnemyShip defeatedShip)
    {
        BeginVictoryPresentation();
    }

    public void BeginVictoryPresentation()
    {
        if(IsCombatEnding) 
        {
            return;
        }

        victoryPresentationInProgress = true;
        CombatResolutionStarted?.Invoke();
        RestoreNormalTime();
    }
}
