using UnityEngine;
using TMPro;

public class NodeMenuPanel : MonoBehaviour
{

    public GameObject panel;
    public TMP_Text  title;
    public TMP_Text  description;

    MapNode currentNode;

    public static NodeMenuPanel Instance;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Open(MapNode node)
    {
        currentNode = node;
        title.text = node.type.ToString();
        description.text = GetDescription(node.type);
        panel.SetActive(true);
    }

    public void EnterNode()
    {
        Debug.Log("Entering node: " + currentNode.type);

        panel.SetActive(false);

        // GAMEPLAY GOES HERE
    }

    public void Cancel()
    {
        panel.SetActive(false);
    }

    string GetDescription(NodeType type)
    {
        switch(type)
        {
            case NodeType.Combat: return "Hostile ship detected.";
            case NodeType.Event: return "Unknown signal.";
            case NodeType.Outpost: return "Neutral outpost spotted.";
            case NodeType.Treasure: return "Radars detect loot with minimal threat ahead.";
            case NodeType.Boss: return "Juggernaught detected.  Proceed with extreme caution.";
            case NodeType.Special: return "??????";
            default: return "";
        }
    }
}
