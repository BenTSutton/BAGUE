using UnityEngine;

[CreateAssetMenu(menuName = "Map/Node Content/Special")]
public class SpecialDefinition : NodeContentDefinition
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