using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Navigator")]
public class NavigatorEffect : CrewEffect
{
    [SerializeField, Min(0)] private int jumpFuelDiscount = 1;

    public override int ModifyJumpFuelCost(int cost)
    {
        return Mathf.Max(0, cost - jumpFuelDiscount);
    }
}
