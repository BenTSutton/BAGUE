using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Rooms/Kitchen Room")]
public class KitchenRoom : Room
{
    // Level 1 = Player gains HP
    // Level 2 = Player healing increased
    // Level 3 = Player gains bonus HP 

    [Header("Player Health")]
    [SerializeField, Min(0)] private int levelOneMaxHealth = 1;
    [SerializeField, Min(0)] private int levelThreeMaxHealth = 2;
    [SerializeField, Min(1f)] private float healingMultiplier = 1.25f;

    public override void OnUpgrade(RoomInstance instance)
    {
    }

    public override string GetUpgradeDescription(int level)
    {
        if (level == 1)
        {
            return $"Increase player maximum health by {levelOneMaxHealth}.";
        }

        if (level == 2)
        {
            float healingIncrease = (healingMultiplier - 1f) * 100f;
            return $"Increase all player healing by {healingIncrease:0.#}%.";
        }

        return $"Increase player maximum health by another {levelThreeMaxHealth}.";
    }

    public int GetPlayerMaxHealthBonus(int level)
    {
        int bonus = 0;

        if (level >= 1)
        {
            bonus += levelOneMaxHealth;
        }

        if (level >= 3)
        {
            bonus += levelThreeMaxHealth;
        }

        return bonus;
    }

    public int ModifyPlayerHealing(int amount, int level)
    {
        if (level >= 2)
        {
            return Mathf.CeilToInt(amount * healingMultiplier);
        }

        return amount;
    }
}
