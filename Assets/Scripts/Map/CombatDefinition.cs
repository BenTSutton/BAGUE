using UnityEngine;


[CreateAssetMenu(menuName = "Map/Node Content/Combat")]
public abstract class CombatDefinition : NodeContentDefinition
{
    public string encounterId;

    public abstract CombatType combatType { get; }

    public override NodeResolutionResult Resolve(NodeState state)
    {
        return new NodeResolutionResult
        {
            title = displayName,
            summary = "Encountered combat: " + encounterId
        };
    }
}