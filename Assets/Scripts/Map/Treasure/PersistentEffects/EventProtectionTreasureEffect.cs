using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Treasures/Persistent Effects/Event Protection")]
public class EventProtectionTreasureEffect : PersistentTreasureEffect
{
    public List<EventEffectType> blockedEffectTypes = new List<EventEffectType>();

    public override bool ProtectsFromEventEffect(EventEffectData effect)
    {
        if (effect == null
            || blockedEffectTypes == null
            || !blockedEffectTypes.Contains(effect.effectType))
        {
            return false;
        }

        return effect.effectType == EventEffectType.DamageShip
            || effect.effectType == EventEffectType.LoseCrew
            || effect.effectType == EventEffectType.LoseCrewAndNextFightHas1HP
            || (effect.effectType == EventEffectType.AddFuel && effect.amount < 0)
            || (effect.effectType == EventEffectType.AddMoney && effect.amount < 0);
    }
}
