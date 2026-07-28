using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Rooms/Shield Room")]
public class ShieldRoom : Room
{
    // Level 1 = Incoming damage to ship reduced
    // Level 2 = Incoming damage to ship reduced further
    // Level 3 = First hit every combat negated completely

    [Header("Hull Protection")]
    [SerializeField, Min(0)] private int levelOneDamageReduction = 1;
    [SerializeField, Min(0)] private int levelTwoDamageReduction = 2;

    public override void OnUpgrade(RoomInstance instance)
    {
    }

    public override string GetUpgradeDescription(int level)
    {
        if (level == 1)
        {
            return $"Reduce incoming ship hull damage by {levelOneDamageReduction}.";
        }

        if (level == 2)
        {
            return $"Increase ship hull damage reduction to {levelTwoDamageReduction}.";
        }

        return "Completely negate the first ship hull hit each combat.";
    }

    public int ModifyIncomingDamage(int damage, int level)
    {
        if (level >= 2)
        {
            return Mathf.Max(0, damage - levelTwoDamageReduction);
        }

        if (level == 1)
        {
            return Mathf.Max(0, damage - levelOneDamageReduction);
        }

        return damage;
    }

    public bool NegatesFirstHit(int level)
    {
        return level >= 3;
    }
}
