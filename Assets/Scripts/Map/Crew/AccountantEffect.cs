using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Accountant")]
public class AccountantEffect : CrewEffect
{
    [SerializeField, Min(0)] private int bonusCredits = 1;

    public override int ModifyMoneyGain(int amount) => amount + bonusCredits;
}
