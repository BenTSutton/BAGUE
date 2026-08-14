using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Hullwright")]
public class HullwrightEffect : CrewEffect
{
    [SerializeField, Range(0f, 0.5f)] private float damageReductionPercent = 0.15f;

    public override int ModifyDamageTaken(int amount)
    {
        if (amount <= 0)
            return 0;

        return Mathf.Max(1, Mathf.FloorToInt(amount * (1f - damageReductionPercent)));
    }
}
