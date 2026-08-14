using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Scavenger")]
public class ScavengerEffect : CrewEffect
{
    [SerializeField, Min(0)] private int scrapAfterCombat = 4;

    public override int GetPostCombatScrapReward()
    {
        return scrapAfterCombat;
    }
}
