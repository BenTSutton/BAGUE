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

    public virtual void ApplyEffect()
    {
        switch (type)
        {
            case TreasureType.Fuel:
                RunManager.Instance.AddFuel(amount);
                break;

            case TreasureType.Scrap:
                RunManager.Instance.AddScrap(amount);;
                break;
            case TreasureType.ScrapFuel:
                RunManager.Instance.AddScrap(amount);;
                RunManager.Instance.AddFuel(amount);
                break;

            case TreasureType.Repair:
                RunManager.Instance.AddHealth(amount);
                break;

            case TreasureType.Crew:
                if (crewReward != null)
                    RunManager.Instance.activeCrew.Add(crewReward);
                break;

            case TreasureType.MaxHP:
                RunManager.Instance.AddMaxHealth(amount);
                break;
            case TreasureType.Credits:
                RunManager.Instance.AddMoney(amount);
                break;

            case TreasureType.Special:
                // If you want anything special and cool :) 
                break;
        }
    }
}