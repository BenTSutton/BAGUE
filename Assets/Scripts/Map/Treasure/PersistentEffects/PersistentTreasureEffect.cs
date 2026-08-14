using UnityEngine;

public abstract class PersistentTreasureEffect : ScriptableObject
{
    public string effectDisplayName;

    public Sprite icon;

    [TextArea]
    public string description;

    public string DisplayName => string.IsNullOrWhiteSpace(effectDisplayName)
        ? name
        : effectDisplayName;

    public virtual int ModifyJumpFuelCost(int cost) => cost;
    public virtual int GetPostCombatHealing() => 0;
    public virtual int ModifyCombatCreditsReward(int amount) => amount;
    public virtual int ModifyCombatScrapReward(int amount) => amount;
    public virtual int ModifyCombatFuelReward(int amount) => amount;
    public virtual bool PreventsFirstHit(int incomingDamage) => false;
    public virtual float ModifyCannonDamage(float damage, int shotNumber) => damage;
    public virtual int ModifyShopPrice(int cost) => cost;
    public virtual bool ProtectsFromEventEffect(EventEffectData effect) => false;
}
