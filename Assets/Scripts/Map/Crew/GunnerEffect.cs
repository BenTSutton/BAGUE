using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Gunner")]
public class GunnerEffect : CrewEffect
{
    [SerializeField, Min(2)] private int shotsPerCritical = 3;
    [SerializeField, Min(1f)] private float criticalMultiplier = 2f;

    public override float ModifyCannonDamage(float amount, int shotNumber)
    {
        return shotNumber % shotsPerCritical == 0 ? amount * criticalMultiplier : amount;
    }
}
