using UnityEngine;

[CreateAssetMenu(menuName = "Map/Treasures/Persistent Effects/Shop Modifier")]
public class ShopTreasureEffect : PersistentTreasureEffect
{
    [SerializeField, Range(0f, 0.9f)] private float discountPercent = 0.1f;

    public override int ModifyShopPrice(int cost)
    {
        return Mathf.CeilToInt(cost * (1f - discountPercent));
    }
}
