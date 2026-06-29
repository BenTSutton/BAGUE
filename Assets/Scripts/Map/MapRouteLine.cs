using UnityEngine;

//This class controls what the line renderer lines between nodes in the map looks like
public class MapRouteLine : MonoBehaviour
{
    LineRenderer glowLine;
    LineRenderer coreLine;

    Material coreMat;
    Material glowMat;

    float scrollOffset;
    bool completed;
    bool accessible = true;

    static readonly Color AccessibleCore = new Color(0.4f, 0.9f, 1f, 1f);
    static readonly Color AccessibleGlow = new Color(0.1f, 0.4f, 1f, 0.35f);
    static readonly Color InaccessibleCore = new Color(0.38f, 0.38f, 0.38f, 0.85f);
    static readonly Color InaccessibleGlow = new Color(0.2f, 0.2f, 0.2f, 0.2f);

    public void Setup(Vector2 start, Vector2 end, Material baseMaterial)
    {
        glowLine = CreateLine("Glow", start, end, 0.32f, 0);
        coreLine = CreateLine("Core", start, end, 0.13f, 1);

        coreMat = new Material(baseMaterial);
        glowMat = new Material(baseMaterial);

        coreLine.material = coreMat;
        glowLine.material = glowMat;

        ApplyVisualState();
    }

    LineRenderer CreateLine(string name, Vector2 start, Vector2 end, float width, int sortingOrder)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform);

        LineRenderer line = obj.AddComponent<LineRenderer>();

        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        line.startWidth = width;
        line.endWidth = width;

        line.useWorldSpace = true;
        line.textureMode = LineTextureMode.Tile;
        line.numCapVertices = 4;
        line.numCornerVertices = 4;
        line.sortingOrder = sortingOrder;

        return line;
    }

    void Update()
    {
        // Only viable routes scroll.
        if (accessible && !completed)
        {
            scrollOffset -= Time.deltaTime * 0.6f;
            coreMat.mainTextureOffset = new Vector2(scrollOffset, 0);
        }

        if (completed)
        {
            float pulse = (Mathf.Sin(Time.time * 2f) + 1f) * 0.5f;

            coreMat.color = Color.Lerp(
                new Color(1f, 0.55f, 0.05f, 1f),
                new Color(1f, 0.9f, 0.35f, 1f),
                pulse
            );

            glowMat.color = new Color(
                1f,
                0.45f,
                0.05f,
                Mathf.Lerp(0.25f, 0.55f, pulse)
            );

            glowLine.startWidth = Mathf.Lerp(0.38f, 0.48f, pulse);
            glowLine.endWidth = glowLine.startWidth;
        }
    }

    public void MarkCompleted()
    {
        completed = true;
        accessible = true;
        ApplyVisualState();
    }

    public void SetAccessible(bool isAccessible)
    {
        accessible = isAccessible;
        ApplyVisualState();
    }

    void ApplyVisualState()
    {
        if (completed)
        {
            coreMat.mainTextureOffset = Vector2.zero;

            coreMat.color = new Color(1f, 0.65f, 0.05f, 1f);
            glowMat.color = new Color(1f, 0.4f, 0.05f, 0.4f);

            coreLine.startWidth = 0.16f;
            coreLine.endWidth = 0.16f;

            glowLine.startWidth = 0.4f;
            glowLine.endWidth = 0.4f;
            return;
        }

        coreMat.color = accessible ? AccessibleCore : InaccessibleCore;
        glowMat.color = accessible ? AccessibleGlow : InaccessibleGlow;

        coreLine.startWidth = accessible ? 0.13f : 0.09f;
        coreLine.endWidth = coreLine.startWidth;

        glowLine.startWidth = accessible ? 0.32f : 0.22f;
        glowLine.endWidth = glowLine.startWidth;

        if (!accessible)
            coreMat.mainTextureOffset = Vector2.zero;
    }
}