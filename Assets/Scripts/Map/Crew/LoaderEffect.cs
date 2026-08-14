using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Loader")]
public class LoaderEffect : CrewEffect
{
    [SerializeField, Min(1)] private int boostedShotsPerCombat = 1;
    [SerializeField, Min(1f)] private float openingDamageMultiplier = 1.5f;

    public override float ModifyCannonDamage(float amount, int shotNumber)
    {
        return shotNumber <= boostedShotsPerCombat
            ? amount * openingDamageMultiplier
            : amount;
    }
}
