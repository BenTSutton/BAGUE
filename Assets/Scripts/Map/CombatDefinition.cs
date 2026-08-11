using UnityEngine;


[CreateAssetMenu(menuName = "Map/Node Content/Combat")]
public abstract class CombatDefinition : NodeContentDefinition
{
    public string encounterId;

    [Header("Victory Rewards")]
    [Min(0)] public int rewardCredits;
    [Min(0)] public int rewardScrap;
    [Min(0)] public int rewardFuel;

    public abstract CombatType combatType { get; }

    [Header("Enemy Faction")]
    [SerializeField] private EnemyFactionProfile enemyFaction;

    public EnemyFactionProfile EnemyFaction => enemyFaction;

    public void GrantVictoryRewards(RunManager runManager)
    {
        if (runManager == null)
            return;

        runManager.GrantCombatRewards(rewardCredits, rewardScrap, rewardFuel);
    }

    public override NodeResolutionResult Resolve(NodeState state)
    {
        return new NodeResolutionResult
        {
            title = displayName,
            summary = "Encountered combat: " + encounterId
        };
    }
}
