using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Accountant")]
public class AccountantEffect : CrewEffect
{
    [SerializeField, Range(0f, 1f)] private float bonusCreditsPercent = 0.2f;

    public override int ModifyMoneyGain(int amount)
    {
        return Mathf.CeilToInt(amount * (1f + bonusCreditsPercent));
    }
}
