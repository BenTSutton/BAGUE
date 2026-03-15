using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadMap()
    {
        SceneManager.LoadScene("MapScene");
    }

    public static void LoadCombat()
    {
        SceneManager.LoadScene("CombatScene");
    }
}
