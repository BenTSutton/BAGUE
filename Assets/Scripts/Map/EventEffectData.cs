using System;
using UnityEngine;

// Every new value must also be implemented in EventDefinition or reported as unsupported.
public enum EventEffectType
{
    HealShip,
    DamageShip,
    AddFuel,
    AddMoney,
    AddCrew,
    MoneyOrDamage,
    DuplicateCrewOrKill,
    CreditsOrFuel,
    LoseCrew,
    LoseCrewAndNextFightHas1HP,
    GainCrew,
    GainRandomCrew,
    AddOrLoseMoney,
    AddScrap,
    CanSeeCombatsBeforeStarting,
    GainRandomRareCrew,
    UnlockEvent,
    RandomEffect,
    GainCrewOrLoseCredits,
    None,
    UpgradeRandomRoom,
    GainOrLoseCredits,
    GainBob,
    SpendCredits,
    SetRunFlag
}

public static class EventEffectSupport
{
    public static bool IsImplemented(EventEffectType effectType)
    {
        switch (effectType)
        {
            case EventEffectType.HealShip:
            case EventEffectType.DamageShip:
            case EventEffectType.AddFuel:
            case EventEffectType.AddMoney:
            case EventEffectType.AddCrew:
            case EventEffectType.MoneyOrDamage:
            case EventEffectType.DuplicateCrewOrKill:
            case EventEffectType.CreditsOrFuel:
            case EventEffectType.LoseCrew:
            case EventEffectType.LoseCrewAndNextFightHas1HP:
            case EventEffectType.GainCrew:
            case EventEffectType.GainRandomCrew:
            case EventEffectType.GainRandomRareCrew:
            case EventEffectType.AddOrLoseMoney:
            case EventEffectType.AddScrap:
            case EventEffectType.CanSeeCombatsBeforeStarting:
            case EventEffectType.RandomEffect:
            case EventEffectType.GainCrewOrLoseCredits:
            case EventEffectType.None:
            case EventEffectType.GainOrLoseCredits:
            case EventEffectType.SpendCredits:
            case EventEffectType.SetRunFlag:
                return true;

            case EventEffectType.UnlockEvent:
            case EventEffectType.UpgradeRandomRoom:
            case EventEffectType.GainBob:
            default:
                return false;
        }
    }
}

[Serializable]
public class EventEffectData
{
    [Tooltip("The effect to apply. New types also need runtime support in EventDefinition.")]
    public EventEffectType effectType;
    [Tooltip("Primary value, such as healing, damage or Credits.")]
    public int amount;
    [Tooltip("Secondary value used by effects with two possible outcomes.")]
    public int secondAmount;
    [Tooltip("Reserved legacy value for effects that need a third number.")]
    public int thirdAmount;
    [Tooltip("Percentage chance of the first outcome for random effects.")]
    public int percentageOdds;
    [Tooltip("Exact crew name used by named recruitment effects.")]
    public string crewName;
    [Tooltip("Stable run flag ID used by SetRunFlag and future flag-aware effects.")]
    public string flagId;
}
