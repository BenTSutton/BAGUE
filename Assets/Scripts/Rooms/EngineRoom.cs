using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Rooms/Engine Room")]
public class EngineRoom : Room
{
    [Header("Dodge Chance")]
    [SerializeField, Range(0f, 100f)] private float levelOneDodgeChance = 5f;
    [SerializeField, Range(0f, 100f)] private float levelTwoDodgeChance = 10f;
    [SerializeField, Range(0f, 100f)] private float levelThreeDodgeChance = 15f;

    [Header("Level Three Cloak Bonus")]
    [SerializeField, Range(0.1f, 1f)] private float cloakCooldownMultiplier = 0.75f;

    public override void OnUpgrade(RoomInstance instance)
    {
    }

    public float GetDodgeChance(int level)
    {
        if (level >= 3)
        {
            return levelThreeDodgeChance;
        }

        if (level == 2)
        {
            return levelTwoDodgeChance;
        }

        if (level == 1)
        {
            return levelOneDodgeChance;
        }

        return 0f;
    }

    public float GetCloakCooldownMultiplier(int level)
    {
        if (level >= 3)
        {
            return cloakCooldownMultiplier;
        }

        return 1f;
    }
}
