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

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        // Listen for when an enemy ship dies
        EnemyShip.OnEnemyShipDeath += HandleEnemyShipDeath;
        RunManager.OnPlayerShipDestroyed += HandlePlayerShipDeath;
    }

    private void OnDisable()
    {
        EnemyShip.OnEnemyShipDeath -= HandleEnemyShipDeath;
        RunManager.OnPlayerShipDestroyed -= HandlePlayerShipDeath;
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
        MusicManager.Instance.PlayMapMusic();
    }

    void EnterCombat()
    {
        Debug.Log("Should enter combat");
        DisableMapObjects();
        SceneManager.LoadScene(sceneName:"CombatScene");
        MusicManager.Instance.PlayCombatMusic();
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
        StartCoroutine(WinCombatRoutine());
    }

    private void LoseCombat()
    {
        StartCoroutine(LoseCombatRoutine());;
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
            MapRunState.Instance.CompleteCurrentNodeAfterEvent(MapRunState.Instance.currentNode);
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

        switch (newState)
        {
            case GameState.Navigation:
                EnterMap();
                break;
            case GameState.Combat:
                Debug.Log("Switched to combat state");
                if (oldState != GameState.Aiming) EnterCombat();
                break;
            case GameState.Aiming:
                Debug.Log("Switched to aiming state");
                break;
        }
    }

    public void EnterCombat(CombatDefinition combatDefinition)
    {
        currentCombatNode = combatDefinition;
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
}
