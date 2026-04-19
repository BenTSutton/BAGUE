using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Preview of the node when you select it from the map
public class NodeMenuPanel : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text title;
    public TMP_Text description;
    public TMP_Text resultText;
    public Button enterButton;

    MapNode currentNode;
    NodeView currentNodeView;

    public static NodeMenuPanel Instance;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    //Select the node in the map
    public void Open(MapNode node, NodeView nodeView)
    {
        SetColorOfPreviousNode();
        currentNode = node;
        currentNodeView = nodeView;

        NodeState state = MapRunState.Instance.GetState(node);

        title.text = node.type.ToString();

        if (state.visited && !string.IsNullOrEmpty(state.resultSummary))
            description.text = state.resultSummary;
        else
            description.text = GetDescription(node.type);

        panel.SetActive(true);

        enterButton.interactable = state.selectable && !state.completed && !state.permanentlyLocked;
    }

    //Trigger the enternode logic
    public void EnterNode()
    {
        if (currentNode == null)
            return;

        NodeState state = MapRunState.Instance.GetState(currentNode);

        if (!state.selectable || state.permanentlyLocked || state.completed)
        {
            Debug.Log("Node cannot be entered.");
            return;
        }

        panel.SetActive(false);
        MapRunState.Instance.EnterNode(currentNode);

        RefreshAllNodeViews();
    }

    //Close the panel
    public void Cancel()
    {
        panel.SetActive(false);
    }

    void SetColorOfPreviousNode()
    {
        if (currentNodeView != null)
        {
            currentNodeView.UpdateColour();
        }
    }

    //Update all nodes to make sure they are correct colour
    public void RefreshAllNodeViews()
    {
        foreach (var nodeView in FindObjectsOfType<NodeView>())
        {
            nodeView.UpdateColour();
        }

        MapShip.Instance.gameObject.transform.position = currentNodeView.gameObject.transform.position;
    }

    /*void CheckIfShouldMoveShipHere(NodeView nodeView)
    {
        if(nodeView.GetNode() == currentNode)
        {
            MapShip.Instance.gameObject.transform.position = nodeView.gameObject.transform.position;
        }
    }*/

    //Strings for the description of all nodes before they are assigned
    string GetDescription(NodeType type)
    {
        switch (type)
        {
            case NodeType.Combat: return "Hostile ship detected.";
            case NodeType.Event: return "A random event, the outcome will depend on your action.";
            case NodeType.Outpost: return "A chance to upgrade your ship, recruit new sailors, and more.";
            case NodeType.Treasure: return "The chance for loot with minimal risk.";
            case NodeType.Boss: return "Juggernaught detected. Proceed with extreme caution.";
            case NodeType.Special: return "??????";
            default: return "";
        }
    }
}