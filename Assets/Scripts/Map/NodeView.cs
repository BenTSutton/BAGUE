using UnityEngine;

public class NodeView : MonoBehaviour
{
    MapNode node;

    [Header("Sprite parts")]
    public SpriteRenderer spriteRenderer;
    public SpriteLoader spriteLoader;

    [Header("Node Colours")]
    public Color selectableColor = Color.white;
    public Color discoveredColor = new Color(0.7f, 0.7f, 0.7f);
    public Color completedColor = new Color(0.3f, 0.8f, 0.3f);
    public Color greyedOutColor = new Color(0.35f, 0.35f, 0.9f);
    public Color hiddenColor = new Color(0f, 0f, 0f, 0.9f);

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
        NodeMenuPanel.Instance.Open(node, this);
    }

    void UpdateColourToSelect()
    {
        spriteRenderer.color = new Color32(13, 189, 16, 255);
    }

    public void UpdateColour()
    {
        NodeState state = MapRunState.Instance.GetState(node);

        GetComponent<Collider2D>().enabled = state.selectable;

        if (state.permanentlyLocked)
        {
            SetGrey();
        }
        else if (state.completed)
        {
            SetCompleted();
        }
        else if (state.selectable)
        {
            SetSelectable();
        }
        else if (state.discovered)
        {
            SetDiscovered();
        }
        else
        {
            SetHidden();
        }
    }

    void SetGrey()
    {
        spriteRenderer.color = greyedOutColor;
    }

    void SetCompleted()
    {
        spriteRenderer.color = completedColor;
    }

    void SetSelectable()
    {
        spriteRenderer.color = selectableColor;
    }

    void SetDiscovered()
    {
        spriteRenderer.color = discoveredColor;
    }

    void SetHidden()
    {
        spriteRenderer.color = hiddenColor;
    }

    public MapNode GetNode()
    {
        return node;
    }

   /* public void UpdateColour()
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
    }*/
}
