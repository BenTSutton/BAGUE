using UnityEngine;

[CreateAssetMenu(menuName = "Map/Treasures/Persistent Effects/Cannon Modifier")]
public class CannonTreasureEffect : PersistentTreasureEffect
{
    [SerializeField, Min(0f)] private float damageMultiplier = 1.15f;

    public override float ModifyCannonDamage(float damage, int shotNumber)
    {
        return damage * damageMultiplier;
    }
}
