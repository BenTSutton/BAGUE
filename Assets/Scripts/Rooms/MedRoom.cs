using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Rooms/Med Room")]
public class MedRoom : Room
{
    // Level 1 = Player gains max health 
    // Level 2 = Player heals periodically
    // Level 3 = Player heals when killing boarders 
    
    [Header("Player Health")]
    [SerializeField, Min(0)] private int maxHealthBonus = 1;
    [SerializeField, Min(0)] private int regenerationAmount = 1;
    [SerializeField, Min(0.1f)] private float regenerationInterval = 10f;
    [SerializeField, Min(0)] private int boarderKillHealing = 1;

    public override void OnUpgrade(RoomInstance instance)
    {
    }

    public override string GetUpgradeDescription(int level)
    {
        if (level == 1)
        {
            return $"Increase player maximum health by {maxHealthBonus}.";
        }

        if (level == 2)
        {
            return $"Heal the player for {regenerationAmount} health every {regenerationInterval:0.#} seconds.";
        }

        return $"Heal the player for {boarderKillHealing} health after killing a boarder.";
    }

    public int GetMaxHealthBonus(int level)
    {
        if (level >= 1)
        {
            return maxHealthBonus;
        }

        return 0;
    }

    public int GetRegenerationAmount(int level)
    {
        if (level >= 2)
        {
            return regenerationAmount;
        }

        return 0;
    }

    public float GetRegenerationInterval()
    {
        return regenerationInterval;
    }

    public int GetBoarderKillHealing(int level)
    {
        if (level >= 3)
        {
            return boarderKillHealing;
        }

        return 0;
    }
}
