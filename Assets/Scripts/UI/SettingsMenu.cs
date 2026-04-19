using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    
    public GameObject settingsPanelObj;
    private bool panelOpen = false;

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ToggleSettingsPanelOpen()
    {
        settingsPanelObj.SetActive(!panelOpen);
        panelOpen = !panelOpen;
    }
}