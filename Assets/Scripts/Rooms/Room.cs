using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Rooms/Room")]
public class Room : ScriptableObject
{
    public string roomName;
    public string roomDescription;
    public Sprite roomLogoSprite;

    public int maxLevel = 3;
    public int baseUpgradeCost = 50;

    public virtual void OnEnter(RoomInstance instance) { }

    public virtual void OnUpgrade(RoomInstance instance) { }

    public virtual string GetUpgradeDescription(int level)
    {
        return roomDescription;
    }

    public int GetUpgradeCost(int currentLevel)
    {
        return (int)Mathf.Max(baseUpgradeCost * currentLevel, baseUpgradeCost * 0.5f);
    }
}
