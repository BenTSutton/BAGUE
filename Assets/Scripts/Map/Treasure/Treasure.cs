using System.Collections.Generic;
using UnityEngine;

public enum TreasureType
{
    Fuel,
    Scrap,
    ScrapFuel,
    Crew,
    Repair,
    Special,
    MaxHP,
    Credits
}

public enum TreasureRarity
{
    Common,
    Uncommon,
    Epic,
    Legendary
}

[CreateAssetMenu(menuName = "Map/Treasures/Treasure")]
public class Treasure : ScriptableObject
{
    public string treasureName;
    public string description;
    public Sprite icon;
    public TreasureType type;
    public TreasureRarity rarity;

    public int amount;
    public CrewMember crewReward;
    public bool purchasable = true;
    public int price;
    public List<PersistentTreasureEffect> persistentEffects =
        new List<PersistentTreasureEffect>();

    public virtual void ApplyEffect()
    {
        if (!TryApplyEffect(out string resultMessage))
            Debug.LogWarning(resultMessage, this);
    }

    public virtual bool TryApplyEffect(out string resultMessage)
    {
        resultMessage = string.Empty;

        RunManager run = RunManager.Instance;
        if (run == null)
        {
            resultMessage = "The treasure could not be applied because the run state is unavailable.";
            return false;
        }

        switch (type)
        {
            case TreasureType.Fuel:
                run.AddFuel(amount);
                break;

            case TreasureType.Scrap:
                run.AddScrap(amount);
                break;

            case TreasureType.ScrapFuel:
                run.AddScrap(amount);
                run.AddFuel(amount);
                break;

            case TreasureType.Repair:
                run.AddHealth(amount);
                break;

            case TreasureType.Crew:
            {
                CrewAcquisitionResult result = run.TryRecruitCrew(crewReward);
                resultMessage = run.GetCrewAcquisitionMessage(result, crewReward);
                if (result != CrewAcquisitionResult.Success)
                    return false;
                break;
            }

            case TreasureType.MaxHP:
                run.AddMaxHealth(amount);
                break;

            case TreasureType.Credits:
                run.AddMoney(amount);
                break;

            case TreasureType.Special:
                break;
        }

        if (persistentEffects != null)
        {
            List<string> addedEffects = new List<string>();
            foreach (PersistentTreasureEffect effect in persistentEffects)
            {
                if (effect == null)
                    continue;

                if (run.AddPersistentTreasureEffect(effect))
                    addedEffects.Add(effect.DisplayName);
            }

            if (addedEffects.Count > 0)
                resultMessage = $"Persistent effect gained: {string.Join(", ", addedEffects)}.";
        }

        return true;
    }
}
