using UnityEngine;

[CreateAssetMenu(menuName = "Map/Treasures/Persistent Effects/Combat Rewards")]
public class CombatRewardTreasureEffect : PersistentTreasureEffect
{
    [SerializeField, Range(0f, 2f)] private float bonusCreditsPercent;
    [SerializeField, Range(0f, 2f)] private float bonusScrapPercent;
    [SerializeField, Min(0)] private int bonusFuel;

    public override int ModifyCombatCreditsReward(int amount)
    {
        return Mathf.CeilToInt(amount * (1f + bonusCreditsPercent));
    }

    public override int ModifyCombatScrapReward(int amount)
    {
        return Mathf.CeilToInt(amount * (1f + bonusScrapPercent));
    }

    public override int ModifyCombatFuelReward(int amount)
    {
        return amount + bonusFuel;
    }
}
