using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class RoomInstance
{
    public Room roomData;
    public bool unlocked;
    public int level = 1;
    public List<CrewMember> assignedCrew = new List<CrewMember>();

    public bool CanUpgrade()
    {
        return unlocked && level < roomData.maxLevel;
    }

    public int GetUpgradeCost()
    {
        return roomData.GetUpgradeCost(level);
    }

    public void Upgrade()
    {
        if (!CanUpgrade()) return;

        level++;
        roomData.OnUpgrade(this);
    }
}