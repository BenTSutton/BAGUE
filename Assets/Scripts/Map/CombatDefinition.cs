using UnityEngine;

[CreateAssetMenu(menuName = "Map/Node Content/Combat")]
public class CombatDefinition : NodeContentDefinition
{
    public string encounterId;

    public override NodeResolutionResult Resolve(NodeState state)
    {
        return new NodeResolutionResult
        {
            title = displayName,
            summary = "Encountered combat: " + encounterId
        };
    }
}