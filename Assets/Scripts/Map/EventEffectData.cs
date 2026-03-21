using System;
using UnityEngine;

//When creating a new effect, add to this and also the logic in EventDefinition.CS
public enum EventEffectType
{
    HealShip,
    DamageShip,
    AddFuel,
    AddMoney,
}
[Serializable]
public class EventEffectData
{
    [Tooltip("Effect type to be shown, this comes from the enum in EventEffectData.cs, any additional options should be added to this and the logic in EventDefinition.cs")]
    public EventEffectType effectType;
    [Tooltip("Value of the effect, in case it is something like a Heal")]
    public int amount;
}