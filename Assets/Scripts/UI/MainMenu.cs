using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{

    public TMP_Text captainNameText;
    public TMP_Text difficultyText;

    public void StartGame()
    {
        GameManager.Instance.StartGame(captainNameText.text, difficultyText.text);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}