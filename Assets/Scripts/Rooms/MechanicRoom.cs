using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Rooms/Mechanic Room")]
public class MechanicRoom : Room
{
    // Level 1 = Heal ship hull after combat
    // Level 2 = Greater ship hull heal
    // Level 3 = Prevent lethal damage to ship once per combat and restore some health

    [Header("Post-Combat Repair")]
    [SerializeField, Min(0)] private int levelOneRepair = 2;
    [SerializeField, Min(0)] private int levelTwoRepair = 5;

    [Header("Level Three Emergency Repair")]
    [SerializeField, Min(1)] private int emergencyRestoreHealth = 5;

    public override void OnUpgrade(RoomInstance instance)
    {
    }

    public override string GetUpgradeDescription(int level)
    {
        if (level == 1)
        {
            return $"Repair {levelOneRepair} ship hull after winning combat.";
        }

        if (level == 2)
        {
            return $"Increase post-combat hull repair to {levelTwoRepair}.";
        }

        return $"Once per combat, prevent lethal ship damage and restore {emergencyRestoreHealth} hull.";
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
