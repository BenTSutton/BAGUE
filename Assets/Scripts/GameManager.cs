using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Navigation,
    Combat
}

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    public GameState currentState;

    public bool combatDebug;

    public GameObject shipUIObj;
    private bool invActive = false;

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
        if(Input.GetKeyDown(KeyCode.I) && currentState == GameState.Navigation)
        {
            ToggleInventory();
        }

        if(Input.GetKeyDown(KeyCode.L) && currentState == GameState.Combat && combatDebug)
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
    
    void ToggleInventory()
    {
        invActive = !invActive;
        shipUIObj.SetActive(invActive);
    }

    public void EnterCombat(CombatDefinition combatDefinition)
    {
        ChangeState(GameState.Combat);
    }
}
