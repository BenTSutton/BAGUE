using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Rooms/Weapons Room")]
public class WeaponsRoom : Room
{
    [Header("Cannon Damage")]
    [SerializeField, Min(0f)] private float levelTwoCannonDamage = 2f;

    public override void OnUpgrade(RoomInstance instance)
    {
    }

    public float ModifyCannonDamage(float baseDamage, int level)
    {
        if (level >= 2)
        {
            return Mathf.Max(baseDamage, levelTwoCannonDamage);
        }

        return baseDamage;
    }
}
