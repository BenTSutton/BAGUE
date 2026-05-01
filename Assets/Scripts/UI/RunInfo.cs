using UnityEngine;
using TMPro;

public class RunInfo : MonoBehaviour
{

    public TMP_Text captainNameText;
    public TMP_Text difficultyText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateCaptainNameText(GameManager.Instance.captainName);
        UpdateDifficultyText(GameManager.Instance.difficulty);
    }

    public void UpdateCaptainNameText(string newText)
    {
        captainNameText.text = "Captain " + newText;
    }

    public void UpdateDifficultyText(string newText)
    {
        difficultyText.text = "Difficulty: " + newText;
    }
}
