using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Mechanic")]
public class MechanicEffect : CrewEffect
{
    [SerializeField, Range(0f, 1f)] private float bonusScrapPercent = 0.25f;

    public override int ModifyScrapGain(int amount)
    {
        return Mathf.CeilToInt(amount * (1f + bonusScrapPercent));
    }
}
