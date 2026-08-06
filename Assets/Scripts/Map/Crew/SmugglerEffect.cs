using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Smuggler")]
public class SmugglerEffect : CrewEffect
{
    [SerializeField, Range(0f, 0.5f)] private float bonusCreditsPercent = 0.1f;
    [SerializeField, Range(0f, 0.5f)] private float shopDiscountPercent = 0.08f;

    public override int ModifyMoneyGain(int amount)
    {
        return Mathf.CeilToInt(amount * (1f + bonusCreditsPercent));
    }

    public override int ModifyPurchaseCost(int cost)
    {
        return Mathf.CeilToInt(cost * (1f - shopDiscountPercent));
    }
}
