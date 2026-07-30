using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Rooms/Helm Room")]
public class HelmRoom : Room
{
    // Level 1 = Less fuel to jump
    // Level 2 = Dodge chance
    // Level 3 = Gain fuel after combat

    [Header("Fuel")]
    [SerializeField, Min(0)] private int jumpFuelReduction = 1;
    [SerializeField, Min(0)] private int postCombatFuel = 2;

    public override void OnUpgrade(RoomInstance instance)
    {
    }

    public override string GetUpgradeDescription(int level)
    {
        if (level == 1)
        {
            return $"Jumps cost {jumpFuelReduction} less fuel.";
        }

        if (level == 2)
        {
            return "Gain 5% ship dodge chance.";
        }

        return $"Gain {postCombatFuel} fuel after winning a combat.";
    }

    public int ModifyJumpFuelCost(int fuelCost, int level)
    {
        if (level >= 1)
        {
            return Mathf.Max(0, fuelCost - jumpFuelReduction);
        }

        return fuelCost;
    }

    public float GetDodgeChance(int level)
    {
        if (level >= 2)
        {
            return 5f;
        }

        return 0f;
    }

    public int GetPostCombatFuel(int level)
    {
        if (level >= 3)
        {
            return postCombatFuel;
        }

        return 0;
    }
}
