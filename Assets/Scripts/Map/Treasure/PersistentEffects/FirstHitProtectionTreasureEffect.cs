using UnityEngine;

[CreateAssetMenu(menuName = "Map/Treasures/Persistent Effects/First-Hit Protection")]
public class FirstHitProtectionTreasureEffect : PersistentTreasureEffect
{
    [SerializeField, Min(1)] private int minimumIncomingDamage = 1;

    public override bool PreventsFirstHit(int incomingDamage)
    {
        return incomingDamage >= minimumIncomingDamage;
    }
}
