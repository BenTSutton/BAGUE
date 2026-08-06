using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Forager")]
public class ForagerEffect : CrewEffect
{
    [SerializeField, Range(0f, 0.5f)] private float bonusFuelPercent = 0.15f;
    [SerializeField, Range(0f, 0.5f)] private float bonusScrapPercent = 0.15f;

    public override int ModifyFuelGain(int amount)
    {
        return Mathf.CeilToInt(amount * (1f + bonusFuelPercent));
    }

    public override int ModifyScrapGain(int amount)
    {
        return Mathf.CeilToInt(amount * (1f + bonusScrapPercent));
    }
}
