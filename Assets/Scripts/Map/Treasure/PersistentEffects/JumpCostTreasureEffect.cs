using UnityEngine;

[CreateAssetMenu(menuName = "Map/Treasures/Persistent Effects/Jump Cost")]
public class JumpCostTreasureEffect : PersistentTreasureEffect
{
    [SerializeField, Min(0)] private int fuelDiscount = 1;

    public override int ModifyJumpFuelCost(int cost)
    {
        return Mathf.Max(0, cost - fuelDiscount);
    }
}
