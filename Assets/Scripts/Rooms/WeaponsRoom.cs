using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Rooms/Weapons Room")]
public class WeaponsRoom : Room
{
    // Level 1 = Cannons deal bonus damage 
    // Level 2 = Cannons deal even more damage
    // Level 3 = Cannons have a chance to deal double damage!

    [Header("Cannon Damage")]
    [SerializeField, Min(0f)] private float levelOneDamageBonus = 0.5f;
    [SerializeField, Min(0f)] private float levelTwoDamageBonus = 2f;
    [SerializeField, Range(0f, 1f)] private float levelThreeDoubleDamageChance = 0.25f;

    public override void OnUpgrade(RoomInstance instance)
    {
    }

    public override string GetUpgradeDescription(int level)
    {
        if (level == 1)
        {
            return $"Cannons deal {levelOneDamageBonus:0.#} bonus damage.";
        }

        if (level == 2)
        {
            return $"Cannons deal another {levelTwoDamageBonus:0.#} bonus damage.";
        }

        return $"Cannon hits have a {levelThreeDoubleDamageChance * 100f:0.#}% chance to deal double damage.";
    }

    public float ModifyCannonDamage(float baseDamage, int level)
    {
        float damage = baseDamage;

        if (level >= 1)
        {
            damage += levelOneDamageBonus;
        }

        if (level >= 2)
        {
            damage += levelTwoDamageBonus;
        }

        if (level >= 3 && Random.value <= levelThreeDoubleDamageChance)
        {
            damage *= 2f;
        }

        return damage;
    }
}
