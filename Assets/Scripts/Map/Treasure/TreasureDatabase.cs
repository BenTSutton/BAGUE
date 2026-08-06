using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Treasures/TreasureDatabase")]
public class TreasureDatabase : ScriptableObject
{
    public List<Treasure> treasures;

    public Treasure GetRandomTreasure(IEnumerable<Treasure> excludedTreasures = null)
    {
        HashSet<Treasure> excluded = BuildExclusionSet(excludedTreasures);
        List<Treasure> availableTreasures = AllConfiguredTreasures()
            .Where(treasure => !excluded.Contains(treasure))
            .Distinct()
            .ToList();

        TreasureRarity rarity = RollDefaultRarity();
        List<Treasure> rarityPool = availableTreasures
            .Where(treasure => treasure.rarity == rarity)
            .ToList();

        return PickRandom(rarityPool.Count > 0 ? rarityPool : availableTreasures);
    }

    public Treasure GetRandomCommonTreasure()
    {
        List<Treasure> allTreasures = AllConfiguredTreasures().ToList();
        List<Treasure> commonTreasures = allTreasures
            .Where(treasure => treasure.rarity == TreasureRarity.Common)
            .ToList();

        return PickRandom(commonTreasures.Count > 0 ? commonTreasures : allTreasures);
    }

    public Treasure GetRandomPurchasableCommonTreasure(
        IEnumerable<Treasure> excludedTreasures,
        IEnumerable<TreasureType> excludedTypes = null)
    {
        HashSet<Treasure> excluded = BuildExclusionSet(excludedTreasures);
        HashSet<TreasureType> typesToAvoid = excludedTypes != null
            ? new HashSet<TreasureType>(excludedTypes)
            : new HashSet<TreasureType>();

        List<Treasure> allCommonOffers = AllConfiguredTreasures()
            .Where(treasure => treasure.purchasable
                && treasure.rarity == TreasureRarity.Common
                && !excluded.Contains(treasure))
            .ToList();
        List<Treasure> preferredOffers = allCommonOffers
            .Where(treasure => !typesToAvoid.Contains(treasure.type))
            .ToList();

        // Prefer a different item type, but fill the slot when stock is limited.
        return PickRandom(preferredOffers.Count > 0 ? preferredOffers : allCommonOffers);
    }

    public Treasure GetRandomPurchasableTreasure(
        IEnumerable<Treasure> excludedTreasures,
        IEnumerable<TreasureType> allowedTypes,
        TreasureRarityWeights rarityWeights,
        IEnumerable<TreasureType> excludedTypes = null)
    {
        HashSet<Treasure> excluded = BuildExclusionSet(excludedTreasures);
        HashSet<TreasureType> allowed = allowedTypes != null
            ? new HashSet<TreasureType>(allowedTypes)
            : new HashSet<TreasureType>();
        HashSet<TreasureType> typesToAvoid = excludedTypes != null
            ? new HashSet<TreasureType>(excludedTypes)
            : new HashSet<TreasureType>();

        List<Treasure> allAllowedOffers = AllConfiguredTreasures()
            .Where(treasure => treasure.purchasable
                && !excluded.Contains(treasure)
                && allowed.Contains(treasure.type))
            .Distinct()
            .ToList();
        List<Treasure> preferredOffers = allAllowedOffers
            .Where(treasure => !typesToAvoid.Contains(treasure.type))
            .ToList();
        List<Treasure> available = preferredOffers.Count > 0
            ? preferredOffers
            : allAllowedOffers;

        if (available.Count == 0)
            return null;

        TreasureRarity? selectedRarity = RollAvailableRarity(available, rarityWeights);
        if (!selectedRarity.HasValue)
            return PickRandom(available);

        List<Treasure> rarityPool = available
            .Where(treasure => treasure.rarity == selectedRarity.Value)
            .ToList();
        return PickRandom(rarityPool);
    }

    private IEnumerable<Treasure> AllConfiguredTreasures()
    {
        return treasures?.Where(treasure => treasure != null)
            ?? Enumerable.Empty<Treasure>();
    }

    private static HashSet<Treasure> BuildExclusionSet(
        IEnumerable<Treasure> excludedTreasures)
    {
        return excludedTreasures != null
            ? new HashSet<Treasure>(excludedTreasures.Where(treasure => treasure != null))
            : new HashSet<Treasure>();
    }

    private static Treasure PickRandom(IList<Treasure> pool)
    {
        return pool.Count == 0
            ? null
            : pool[Random.Range(0, pool.Count)];
    }

    private static TreasureRarity RollDefaultRarity()
    {
        float roll = Random.value;
        if (roll < 0.80f)
            return TreasureRarity.Common;
        if (roll < 0.96f)
            return TreasureRarity.Uncommon;
        return roll < 0.995f
            ? TreasureRarity.Epic
            : TreasureRarity.Legendary;
    }

    private static TreasureRarity? RollAvailableRarity(
        IEnumerable<Treasure> availableTreasures,
        TreasureRarityWeights configuredWeights)
    {
        TreasureRarityWeights weights = configuredWeights ?? new TreasureRarityWeights();
        HashSet<TreasureRarity> availableRarities = new HashSet<TreasureRarity>(
            availableTreasures.Select(treasure => treasure.rarity));
        List<TreasureRarity> weightedRarities = System.Enum
            .GetValues(typeof(TreasureRarity))
            .Cast<TreasureRarity>()
            .Where(rarity => availableRarities.Contains(rarity)
                && weights.GetWeight(rarity) > 0f)
            .ToList();

        if (weightedRarities.Count == 0)
            return null;

        float roll = Random.value * weightedRarities.Sum(weights.GetWeight);
        foreach (TreasureRarity rarity in weightedRarities)
        {
            roll -= weights.GetWeight(rarity);
            if (roll <= 0f)
                return rarity;
        }

        return weightedRarities[weightedRarities.Count - 1];
    }
}
