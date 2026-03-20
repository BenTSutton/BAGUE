using UnityEngine;

[CreateAssetMenu(menuName = "Map/Node Content/Boss")]
public class BossDefinition : NodeContentDefinition
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