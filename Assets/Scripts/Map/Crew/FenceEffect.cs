using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Fence")]
public class FenceEffect : CrewEffect
{
    [SerializeField, Range(0f, 0.9f)] private float purchaseDiscountPercent = 0.15f;

    public override int ModifyPurchaseCost(int cost)
    {
        return Mathf.CeilToInt(cost * (1f - purchaseDiscountPercent));
    }
}
