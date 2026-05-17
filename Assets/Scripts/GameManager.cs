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
        SceneManager.LoadScene(sceneName:"bs-CombatScene");
        MusicManager.Instance.PlayCombatMusic();
    }

    void Update()
    {

        if(Input.GetKeyDown(KeyCode.L) && currentState == GameState.Combat && debug)
        {
            WinCombat();
        }
    }

    void WinCombat()
    {
        ChangeState(GameState.Navigation);
        MapRunState.Instance.CompleteCurrentNodeAfterEvent(MapRunState.Instance.currentNode);
    }

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
}
