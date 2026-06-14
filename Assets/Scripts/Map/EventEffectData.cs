using System;
using UnityEngine;

//When creating a new effect, add to this and also the logic in EventDefinition.CS
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
    GainBob
}
[Serializable]
public class EventEffectData
{
    [Tooltip("Effect type to be shown, this comes from the enum in EventEffectData.cs, any additional options should be added to this and the logic in EventDefinition.cs")]
    public EventEffectType effectType;
    [Tooltip("Value of the effect, in case it is something like a Heal")]
    public int amount;
    [Tooltip("In case the effect needs 2 values")]
    public int secondAmount;
    [Tooltip("In case the effect needs 3 values")]
    public int thirdAmount;
    [Tooltip("If the effect has a percentage chance, edit the chance for the first effect here")]
    public int percentageOdds;
    [Tooltip("Crew name, in case the effect is to add crew")]
    public string crewName;
}