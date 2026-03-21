using UnityEngine;

public enum GameState
{
    Navigation,
    Combat
}

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    public GameState currentState;

    void Awake()
    {
        Instance = this;
    }

    void EnterMap()
    {
        Debug.Log("Should move to map");
    }

    void EnterCombat()
    {
        Debug.Log("Should enter combat");
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
}
