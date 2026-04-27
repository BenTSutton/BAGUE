using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Menu,
    Navigation,
    Combat
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
    }

    void EnterCombat()
    {
        Debug.Log("Should enter combat");
        SceneManager.LoadScene(sceneName:"CombatScene");
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
        currentState = newState;

        switch (newState)
        {
            case GameState.Navigation:
                EnterMap();
                break;
            case GameState.Combat:
                EnterCombat();
                break;
        }
    }

    public void EnterCombat(CombatDefinition combatDefinition)
    {
        ChangeState(GameState.Combat);
    }
}
