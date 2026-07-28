using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class RoomUI : MonoBehaviour
{

    public Room room;
    public RoomInstance roomInstance;
    public Image roomLogo;
    public TMP_Text tooltipNameText;
    public TMP_Text tooltipDescText;
    public TMP_Text tooltipCostText;
    public Button upgradeButton;

    public GameObject level1Obj;
    public GameObject level2Obj;
    public GameObject level3Obj;

    void OnEnable()
    {
        roomInstance = RunManager.Instance.GetRoomInstance(room);
        UpdateRoomUI();
    }

    string ConstructCostText(int curLevel)
    {
        string toConstruct = "";
        if(curLevel == room.maxLevel)
        {
            toConstruct = "Max level!";
        }
        else
        {
            curLevel++;
            toConstruct = "Level " + curLevel.ToString() + ": " + roomInstance.GetUpgradeCost();
        }
        return toConstruct;
    }

    string ConstructDescriptionText(int curLevel)
    {
        if (curLevel == room.maxLevel)
        {
            return "Maximum upgrade reached.";
        }

        int nextLevel = curLevel + 1;
        string upgradeDescription = room.GetUpgradeDescription(nextLevel);

        return "Next level: " + ": " + upgradeDescription;
    }

    void CheckUpgradeButtonShouldBeEnabled(int curLevel)
    {
        if(curLevel == room.maxLevel || (RunManager.Instance.scrap < roomInstance.GetUpgradeCost()))
        {
            upgradeButton.interactable = false;
        }
        else
        {
            upgradeButton.interactable = true;
        }
    }

    void UpdateRoomUI()
    {
        int curLevel = roomInstance.level;
        roomLogo.sprite = room.roomLogoSprite;   
        tooltipDescText.text = ConstructDescriptionText(curLevel);
        tooltipNameText.text = room.roomName + " - Level " + curLevel.ToString();
        tooltipCostText.text = ConstructCostText(curLevel);
        UpdateLevelObjects(curLevel);
        CheckUpgradeButtonShouldBeEnabled(curLevel);
    }

    public void UpgradeRoom()
    {
        RunManager.Instance.UpgradeRoom(room);
        UpdateRoomUI();
    }

    void UpdateLevelObjects(int currentLevel)
    {
        level1Obj.SetActive(currentLevel >= 1);
        level2Obj.SetActive(currentLevel >= 2);
        level3Obj.SetActive(currentLevel >= 3);
    }
}
