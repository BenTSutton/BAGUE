using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Reckless Gunner")]
public class RecklessGunnerEffect : CrewEffect
{
    [SerializeField, Min(0f)] private float damagedShipBonus = 1f;

    public override float ModifyCannonDamage(float amount, int shotNumber)
    {
        RunManager run = RunManager.Instance;
        return run.currentShipHealth * 2 <= run.maxShipHealth
            ? amount + damagedShipBonus
            : amount;
    }
}
