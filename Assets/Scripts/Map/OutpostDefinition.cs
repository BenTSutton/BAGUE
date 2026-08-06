using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum OutpostStockCategory
{
    None = 0,
    Fuel = 1 << 0,
    Scrap = 1 << 1,
    MixedResources = 1 << 2,
    Repair = 1 << 3,
    MaxHealth = 1 << 4,
    Credits = 1 << 5,
    Special = 1 << 6,
    Crew = 1 << 7,
    LegacyAll = Fuel | Scrap | MixedResources | Repair | MaxHealth | Credits | Special | Crew
}

[Flags]
public enum OutpostServiceType
{
    None = 0,
    Repair = 1 << 0,
    Refuel = 1 << 1,
    Reroll = 1 << 2
}

[Serializable]
public class TreasureRarityWeights
{
    [Min(0f)] public float common = 1f;
    [Min(0f)] public float uncommon;
    [Min(0f)] public float epic;
    [Min(0f)] public float legendary;

    public float GetWeight(TreasureRarity rarity)
    {
        return rarity switch
        {
            TreasureRarity.Common => common,
            TreasureRarity.Uncommon => uncommon,
            TreasureRarity.Epic => epic,
            TreasureRarity.Legendary => legendary,
            _ => 0f
        };
    }
}

[CreateAssetMenu(menuName = "Map/Node Content/Outpost")]
public class OutpostDefinition : NodeContentDefinition
{
    [TextArea] public string outcomeText;

    [Header("Stock")]
    public OutpostStockCategory stockCategories = OutpostStockCategory.LegacyAll;
    [Range(0f, 1f)] public float crewOfferProbability = 1f;
    public TreasureRarityWeights rarityWeights = new TreasureRarityWeights();
    [Min(0.01f)] public float priceMultiplier = 1f;

    [Header("Services")]
    public OutpostServiceType services;
    [Min(0)] public int repairServiceCost = 20;
    [Min(1)] public int repairServiceAmount = 15;
    [Min(0)] public int refuelServiceCost = 15;
    [Min(1)] public int refuelServiceAmount = 10;
    [Min(0)] public int rerollCost = 10;

    [SerializeField, HideInInspector] private int frameworkVersion = 1;

    public bool EnsureFrameworkDefaults()
    {
        if (frameworkVersion >= 1)
            return false;

        stockCategories = OutpostStockCategory.LegacyAll;
        crewOfferProbability = 1f;
        rarityWeights = new TreasureRarityWeights();
        priceMultiplier = 1f;
        services = OutpostServiceType.None;
        frameworkVersion = 1;
        return true;
    }

    public bool AllowsCrew =>
        (stockCategories & OutpostStockCategory.Crew) != 0;

    public bool HasService(OutpostServiceType service)
    {
        return (services & service) != 0;
    }

    public IEnumerable<TreasureType> GetAllowedTreasureTypes()
    {
        if ((stockCategories & OutpostStockCategory.Fuel) != 0)
            yield return TreasureType.Fuel;
        if ((stockCategories & OutpostStockCategory.Scrap) != 0)
            yield return TreasureType.Scrap;
        if ((stockCategories & OutpostStockCategory.MixedResources) != 0)
            yield return TreasureType.ScrapFuel;
        if ((stockCategories & OutpostStockCategory.Repair) != 0)
            yield return TreasureType.Repair;
        if ((stockCategories & OutpostStockCategory.MaxHealth) != 0)
            yield return TreasureType.MaxHP;
        if ((stockCategories & OutpostStockCategory.Credits) != 0)
            yield return TreasureType.Credits;
        if ((stockCategories & OutpostStockCategory.Special) != 0)
            yield return TreasureType.Special;
    }

    public override NodeResolutionResult Resolve(NodeState state)
    {
        return new NodeResolutionResult
        {
            title = displayName,
            summary = outcomeText
        };
    }
}
