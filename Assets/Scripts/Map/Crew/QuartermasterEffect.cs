using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Quartermaster")]
public class QuartermasterEffect : CrewEffect
{
    [SerializeField, Min(0)] private int fuelDiscount = 1;

    public override int ModifyJumpFuelCost(int cost) => cost - fuelDiscount;
}
