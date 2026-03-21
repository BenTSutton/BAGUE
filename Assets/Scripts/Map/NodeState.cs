using System;

[Serializable]
public class NodeState
{
    public MapNode node;

    public bool discovered;
    public bool visited;
    public bool completed;

    public bool selectable;
    public bool permanentlyLocked;

    public string generatedContentId;   // What type of node this will be
    public string resultSummary;        // What to show when clicking this node after it is visited

    public bool eventChoiceMade;
    public int chosenOptionIndex = -1;
}