using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Rooms/Deck Room")]
public class DeckRoom : Room
{
    // Level 1 = Boarders stunned upon entry 
    // Level 2 = Spawn with 1 less health 
    // Level 3 = Boarders take damage periodically 

    [Header("Boarder Stun")]
    [SerializeField, Min(0f)] private float boarderEntryStunDuration = 5f;

    [Header("Boarder Damage")]
    [SerializeField, Min(0)] private int periodicDamage = 1;
    [SerializeField, Min(0f)] private float periodicDamageInterval = 5f;

    [Header("Boarder Health Reduction")]
    [SerializeField, Min(0)] private int boarderHealthReduction = 1;

    public override void OnUpgrade(RoomInstance instance)
    {
    }

    public override string GetUpgradeDescription(int level)
    {
        if (level == 1)
        {
            return $"Boarders are stunned for {boarderEntryStunDuration:0.#} seconds when they enter your ship.";
        }

        if (level == 2)
        {
            return $"Boarders enter the ship with {boarderHealthReduction} less health.";
        }

        return $"Boarders take {periodicDamage} damage every {periodicDamageInterval:0.#} seconds.";
    }

    public float GetBoarderEntryStunDuration(int level)
    {
        if (level >= 1)
        {
            return boarderEntryStunDuration;
        }

        return 0f;
    }

    public int GetPeriodicDamage(int level)
    {
        if (level >= 3)
        {
            return periodicDamage;
        }

        return 0;
    }

    public float GetPeriodicDamageInterval()
    {
        return periodicDamageInterval;
    }

    public int GetBoarderHealthReduction(int level)
    {
        if (level >= 2)
        {
            return boarderHealthReduction;
        }

        return 0;
    }
}
