using UnityEngine;

[CreateAssetMenu(menuName = "Map/Treasures/Persistent Effects/Post-Combat Healing")]
public class PostCombatHealingTreasureEffect : PersistentTreasureEffect
{
    [SerializeField, Min(0)] private int healingAfterCombat = 10;

    public override int GetPostCombatHealing()
    {
        return healingAfterCombat;
    }
}
