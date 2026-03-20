using UnityEngine;

[CreateAssetMenu(menuName = "Map/Node Content/Treasure")]
public class TreasureDefinition : NodeContentDefinition
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