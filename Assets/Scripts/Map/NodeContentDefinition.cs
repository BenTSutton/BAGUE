using UnityEngine;
using UnityEngine;

//Base class for a node describing what content it should have
public abstract class NodeContentDefinition : ScriptableObject
{
    // Arbitary number, can be anything
    [Tooltip("Unique ID, not important what it is as long as it is unique")]
    public string id;

    [Tooltip("Name to be shown in the preview of the node dialog")]
    public string displayName;

    public abstract NodeResolutionResult Resolve(NodeState state);
}