using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Rooms/Mechanic Room")]
public class MechanicRoom : Room
{
    [Header("Post-Combat Repair")]
    [SerializeField, Min(0)] private int levelOneRepair = 2;
    [SerializeField, Min(0)] private int levelTwoRepair = 5;

    [Header("Level Three Emergency Repair")]
    [SerializeField, Min(1)] private int emergencyRestoreHealth = 5;

    public override void OnUpgrade(RoomInstance instance)
    {
    }

    public int GetPostCombatRepair(int level)
    {
        if (level >= 2)
        {
            return levelTwoRepair;
        }

        if (level == 1)
        {
            return levelOneRepair;
        }

        return 0;
    }

    public int GetEmergencyRestoreHealth(int level)
    {
        if (level >= 3)
        {
            return emergencyRestoreHealth;
        }

        return 0;
    }
}
