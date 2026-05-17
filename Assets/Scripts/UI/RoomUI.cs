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
        tooltipDescText.text = room.roomDescription;
        tooltipNameText.text = room.roomName + " - Level " + curLevel.ToString();
        tooltipCostText.text = ConstructCostText(curLevel);
        CheckUpgradeButtonShouldBeEnabled(curLevel);
    }

    public void UpgradeRoom()
    {
        RunManager.Instance.UpgradeRoom(room);
        UpdateRoomUI();
    }
}
