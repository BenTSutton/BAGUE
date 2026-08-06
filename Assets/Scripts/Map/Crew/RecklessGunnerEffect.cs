using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Reckless Gunner")]
public class RecklessGunnerEffect : CrewEffect
{
    [SerializeField, Min(1f)] private float damagedShipMultiplier = 1.35f;

    public override float ModifyCannonDamage(float amount, int shotNumber)
    {
        RunManager run = RunManager.Instance;
        return run.currentShipHealth * 2 <= run.maxShipHealth
            ? amount * damagedShipMultiplier
            : amount;
    }
}
