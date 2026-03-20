using UnityEngine;

[CreateAssetMenu(menuName = "Map/Node Content/Outpost")]
public class OutpostDefinition : NodeContentDefinition
{
    [TextArea] public string outcomeText;

    public override NodeResolutionResult Resolve(NodeState state)
    {
        return new NodeResolutionResult
        {
            title = displayName,
            summary = outcomeText
        };
    }
}