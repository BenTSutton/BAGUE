using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Rooms/Bunk Room")]
public class BunkRoom : Room
{
    // Level 1 = Bonus Credits
    // Level 2 = Additional Bonus Credits
    // Level 3 = Bonus Credits + Scrap!
    [Header("Resource Bonuses")]
    [SerializeField, Range(0f, 1f)] private float levelOneBonus = 0.05f;
    [SerializeField, Range(0f, 1f)] private float levelTwoBonus = 0.10f;
    [SerializeField, Range(0f, 1f)] private float levelThreeBonus = 0.15f;

    public override void OnUpgrade(RoomInstance instance)
    {
    }

    public override string GetUpgradeDescription(int level)
    {
        if (level == 1)
        {
            return $"Gain {levelOneBonus * 100f:0.#}% more credits.";
        }

        if (level == 2)
        {
            return $"Gain {levelTwoBonus * 100f:0.#}% more credits.";
        }

        return $"Gain {levelThreeBonus * 100f:0.#}% more credits and scrap.";
    }

    public int ModifyMoneyGain(int amount, int level)
    {
        return Mathf.RoundToInt(amount * GetResourceMultiplier(level));
    }

    public int ModifyScrapGain(int amount, int level)
    {
        if (level >= 3)
        {
            return Mathf.RoundToInt(amount * (1f + levelThreeBonus));
        }

        return amount;
    }

    private float GetResourceMultiplier(int level)
    {
        if (level >= 3)
        {
            return 1f + levelThreeBonus;
        }

        if (level == 2)
        {
            return 1f + levelTwoBonus;
        }

        if (level == 1)
        {
            return 1f + levelOneBonus;
        }

        return 1f;
    }
}
