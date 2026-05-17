using UnityEngine;
using TMPro;

public class RunInfo : MonoBehaviour
{

    public TMP_Text captainNameText;
    public TMP_Text difficultyText;
    public TMP_Text levelText;
    public TMP_Text nodeText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateCaptainNameText(GameManager.Instance.captainName);
        UpdateDifficultyText(GameManager.Instance.difficulty);
        UpdateLevelText(RunManager.Instance.level.ToString());
        if(MapRunState.Instance.currentNode != null)
        {
            UpdateNodeText(MapRunState.Instance.currentNode.index.ToString());
        }
    }

    public void UpdateCaptainNameText(string newText)
    {
        captainNameText.text = "Captain " + newText;
    }

    public void UpdateDifficultyText(string newText)
    {
        difficultyText.text = "Difficulty: " + newText;
    }

    public void UpdateLevelText(string newText)
    {
        levelText.text = "Sector: " + newText;
    }

    public void UpdateNodeText(string newText)
    {
        nodeText.text = "Node: " + newText + " / 10";
    }
}
