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

    public string generatedContentId;
    public string resultSummary;

    public bool eventChoiceMade;
    public int chosenOptionIndex = -1;

    public bool combatRewardsGranted;
}
