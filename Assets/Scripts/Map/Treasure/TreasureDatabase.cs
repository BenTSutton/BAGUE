using System.Collections.Generic;
using System.Linq;
using UnityEngine;


//Database of node content, make sure this is updated for all new nodes created
[CreateAssetMenu(menuName = "Map/Treasures/TreasureDatabase")]
public class TreasureDatabase : ScriptableObject
{
    public List<Treasure> treasures;

    public Treasure GetRandomTreasure()
    {
        float roll = Random.value; 

        TreasureRarity chosenRarity;

        if (roll < 0.70f)
            chosenRarity = TreasureRarity.Common;
        else if (roll < 0.90f)
            chosenRarity = TreasureRarity.Uncommon;
        else if (roll < 0.99f)
            chosenRarity = TreasureRarity.Epic;
        else
            chosenRarity = TreasureRarity.Legendary;

        List<Treasure> pool = treasures
            .Where(t => t.rarity == chosenRarity)
            .ToList();

        // No treasures of 1 kind for example
        if (pool.Count == 0)
        {
            pool = treasures;
        }

        return pool[Random.Range(0, pool.Count)];
    }

    public Treasure GetRandomCommonTreasure()
    {
        List<Treasure> pool = treasures
            .Where(t => t.rarity == TreasureRarity.Common)
            .ToList();

        // No treasures of 1 kind for example
        if (pool.Count == 0)
        {
            pool = treasures;
        }

        return pool[Random.Range(0, pool.Count)];
    }

    public Treasure GetRandomPurchasableCommonTreasure(
        IEnumerable<Treasure> excludedTreasures,
        IEnumerable<TreasureType> excludedTypes = null)
    {
        HashSet<Treasure> excluded = excludedTreasures != null
            ? new HashSet<Treasure>(excludedTreasures)
            : new HashSet<Treasure>();
        HashSet<TreasureType> typesToAvoid = excludedTypes != null
            ? new HashSet<TreasureType>(excludedTypes)
            : new HashSet<TreasureType>();

        List<Treasure> pool = treasures
            .Where(treasure => treasure != null
                && treasure.purchasable
                && treasure.rarity == TreasureRarity.Common
                && !excluded.Contains(treasure)
                && !typesToAvoid.Contains(treasure.type))
            .ToList();

        // Prefer a different item type, but still fill the slot if there is only one type.
        if (pool.Count == 0)
        {
            pool = treasures
                .Where(treasure => treasure != null
                    && treasure.purchasable
                    && treasure.rarity == TreasureRarity.Common
                    && !excluded.Contains(treasure))
                .ToList();
        }

        if (pool.Count == 0)
            return null;

        return pool[Random.Range(0, pool.Count)];
    }
}
