using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NodeMenuPanel : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text title;
    public TMP_Text description;
    public TMP_Text fuelCostText;
    public TMP_Text resultText;
    public Button enterButton;

    private MapNode currentNode;
    private NodeView currentNodeView;

    public static NodeMenuPanel Instance;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Open(MapNode node, NodeView nodeView)
    {
        SFXManager.Instance?.PlayShowNodePanel();
        SetColorOfPreviousNode();
        currentNode = node;
        currentNodeView = nodeView;

        NodeState state = MapRunState.Instance.GetState(node);

        title.text = node.type.ToString();

        string nodeDescription = state.visited && !string.IsNullOrEmpty(state.resultSummary)
            ? state.resultSummary
            : GetDescription(node.type);

        int fuelCost = MapRunState.Instance.GetTravelFuelCost(node);
        bool canAffordTravel = MapRunState.Instance.CanAffordTravelTo(node);

        if (fuelCost == 0)
        {
            fuelCostText.text = $"Jump Cost: Free";
        }
        else if (canAffordTravel)
        {
            fuelCostText.text = $"Jump Cost: {fuelCost} Fuel";
        }
        else
        {
            fuelCostText.text = $"Not enough fuel: {fuelCost} required, "
                + $"{RunManager.Instance.fuel} available.";
        }

        description.text = nodeDescription;

        PanelAnimation.Open(panel);

        enterButton.interactable = state.selectable
            && !state.completed
            && !state.permanentlyLocked
            && canAffordTravel;
    }

    public void EnterNode()
    {
        if (currentNode == null)
            return;

        NodeState state = MapRunState.Instance.GetState(currentNode);

        if (!state.selectable || state.permanentlyLocked || state.completed)
        {
            Debug.Log("Node cannot be entered.");
            SFXManager.Instance?.PlayNegative();
            return;
        }

        if (!MapRunState.Instance.EnterNode(currentNode))
        {
            SFXManager.Instance?.PlayNegative();
            Open(currentNode, currentNodeView);
            return;
        }

        SFXManager.Instance?.PlayTravel();
        PanelAnimation.Close(panel);

        RefreshAllNodeViews(true);
    }

    public void Cancel()
    {
        SFXManager.Instance?.PlayCancel();
        PanelAnimation.Close(panel);
    }

    void SetColorOfPreviousNode()
    {
        if (currentNodeView != null)
            currentNodeView.UpdateColour();
    }

    public void RefreshAllNodeViews(bool moveShip)
    {
        foreach (NodeView nodeView in FindObjectsByType<NodeView>(FindObjectsSortMode.None))
            nodeView.UpdateColour();

        if (moveShip && currentNodeView != null && MapShip.Instance != null)
            MapShip.Instance.transform.position = currentNodeView.transform.position;
    }

    private static string GetDescription(NodeType type)
    {
        return type switch
        {
            NodeType.Combat => "Hostile ship detected. An opportunity for plunder, if you succeed.",
            NodeType.Event => "A random event whose outcome depends on your action.",
            NodeType.Outpost => "Upgrade your ship, buy supplies, or recruit crew.",
            NodeType.Treasure => "A chance for loot with minimal risk.",
            NodeType.Boss => "Juggernaught detected. Proceed with extreme caution.",
            NodeType.Special => "??????",
            _ => string.Empty
        };
    }
}
