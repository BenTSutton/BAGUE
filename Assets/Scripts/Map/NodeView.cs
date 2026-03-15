using UnityEngine;

public class NodeView : MonoBehaviour
{
    MapNode node;

    public SpriteRenderer spriteRenderer;
    public SpriteLoader spriteLoader;

    public void UpdateSprite(Sprite newSprite)
    {
        spriteRenderer.sprite = newSprite;
        UpdateColour();
    }

    public void Initialize(MapNode nodeData)
    {
        node = nodeData;
        spriteLoader = GameObject.Find("GameManager").GetComponent<SpriteLoader>();
        UpdateSprite(spriteLoader.sprites[node.type.ToString()]);
    }

    void OnMouseDown()
    {
        NodeMenuPanel.Instance.Open(node);
        SelectNode();
    }

    void SelectNode()
    {
        if (node.visited)
            return;
        
        node.visited = true;

        Debug.Log("Node selected: " + node.type);

        if(node.type == NodeType.Combat)
        {
            GameManager.Instance.ChangeState(GameState.Combat);
        }
    }

    void UpdateColour()
    {
        switch(node.type)
        {
            case NodeType.Combat:
                spriteRenderer.color = new Color32(176, 25, 14, 255);
                break;
            case NodeType.Event:
                spriteRenderer.color = new Color32(214, 198, 21, 255);
                break;
            case NodeType.Outpost:
                spriteRenderer.color = new Color32(7, 5, 31, 255);
                break;
            case NodeType.Treasure:
                spriteRenderer.color = new Color32(79, 67, 7, 255);
                break;
            case NodeType.Boss:
                spriteRenderer.color = new Color32(82, 1, 29, 255);
                break;
            case NodeType.Special:
                spriteRenderer.color = new Color32(37, 226, 247, 255);
                break;
        }
    }
}
