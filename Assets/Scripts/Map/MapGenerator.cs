using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Size")]
    public int layers = 10;
    public int width = 4;
    public int pathCount = 6;

    [Header("Layout")]
    public float xSpacing = 2f;
    public float ySpacing = 1.5f;

    [Header("Prefabs")]
    public GameObject nodePrefab;
    public Material lineMaterial;

    private List<List<MapNode>> map = new List<List<MapNode>>();
    private HashSet<MapNode> reachableNodes = new HashSet<MapNode>();
    private List<MapNode> startNodes = new List<MapNode>();

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        ClearOldMap();

        CreateGrid();
        GeneratePaths();
        FindReachableNodes();
        AssignTypes();
        SpawnNodes();
        DrawConnections();
    }

    void ClearOldMap()
    {
        map.Clear();
        reachableNodes.Clear();
        startNodes.Clear();

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void CreateGrid()
    {
        for (int y = 0; y < layers; y++)
        {
            List<MapNode> row = new List<MapNode>();

            for (int x = 0; x < width; x++)
            {
                MapNode node = new MapNode();
                node.layer = y;
                node.index = x;
                node.position = new Vector2((x - (width - 1) / 2f) * xSpacing, y * ySpacing);

                row.Add(node);
            }

            map.Add(row);
        }
    }

    void GeneratePaths()
    {
        // Pick unique starting columns
        List<int> startColumns = PickStartColumns(Mathf.Min(pathCount, width));

        foreach (int col in startColumns)
        {
            startNodes.Add(map[0][col]);
        }

        // If pathCount > width, reuse start columns with random picks
        for (int p = 0; p < pathCount; p++)
        {
            int currentX = startColumns[p % startColumns.Count];

            for (int y = 0; y < layers - 1; y++)
            {
                MapNode current = map[y][currentX];

                List<int> candidates = GetCandidateNextXs(currentX);

                int nextX = ChooseBestNextX(y, currentX, candidates);
                MapNode next = map[y + 1][nextX];

                if (!current.connections.Contains(next))
                {
                    current.connections.Add(next);
                }

                currentX = nextX;
            }
        }

        EnsureAllStartsHaveConnection();
    }

    List<int> PickStartColumns(int count)
    {
        List<int> all = Enumerable.Range(0, width).ToList();

        // Shuffle
        for (int i = 0; i < all.Count; i++)
        {
            int swap = Random.Range(i, all.Count);
            (all[i], all[swap]) = (all[swap], all[i]);
        }

        return all.Take(count).OrderBy(x => x).ToList();
    }

    List<int> GetCandidateNextXs(int currentX)
    {
        List<int> result = new List<int>();

        for (int dx = -1; dx <= 1; dx++)
        {
            int nextX = currentX + dx;
            if (nextX >= 0 && nextX < width)
            {
                result.Add(nextX);
            }
        }

        return result;
    }

    int ChooseBestNextX(int layer, int currentX, List<int> candidates)
    {
        // Prefer straight, then slight movement, and avoid crossings
        List<int> valid = new List<int>();

        foreach (int nextX in candidates)
        {
            if (!WouldCrossExistingConnection(layer, currentX, nextX))
            {
                valid.Add(nextX);
            }
        }

        // If everything crosses, allow candidates anyway so generation never stalls
        if (valid.Count == 0)
        {
            valid = candidates;
        }

        // Weighted preference: straight > diagonal
        List<int> weighted = new List<int>();
        foreach (int nextX in valid)
        {
            int distance = Mathf.Abs(nextX - currentX);

            if (distance == 0)
            {
                weighted.Add(nextX);
                weighted.Add(nextX);
                weighted.Add(nextX);
            }
            else
            {
                weighted.Add(nextX);
            }
        }

        return weighted[Random.Range(0, weighted.Count)];
    }

    bool WouldCrossExistingConnection(int layer, int fromX, int toX)
    {
        // Check every connection already created between this layer and the next.
        // Crossing happens when relative ordering flips:
        // existingFrom < fromX but existingTo > toX
        // or existingFrom > fromX but existingTo < toX
        for (int x = 0; x < width; x++)
        {
            MapNode node = map[layer][x];

            foreach (MapNode connected in node.connections)
            {
                if (connected.layer != layer + 1)
                    continue;

                int existingFrom = node.index;
                int existingTo = connected.index;

                bool crosses =
                    (existingFrom < fromX && existingTo > toX) ||
                    (existingFrom > fromX && existingTo < toX);

                if (crosses)
                    return true;
            }
        }

        return false;
    }

    void EnsureAllStartsHaveConnection()
    {
        foreach (MapNode start in startNodes)
        {
            if (start.connections.Count > 0)
                continue;

            int nextX = start.index;
            MapNode next = map[1][nextX];
            start.connections.Add(next);
        }
    }

    void FindReachableNodes()
    {
        reachableNodes.Clear();

        foreach (var start in startNodes)
        {
            Traverse(start);
        }
    }

    void Traverse(MapNode node)
    {
        if (reachableNodes.Contains(node))
            return;

        reachableNodes.Add(node);

        foreach (var next in node.connections)
        {
            Traverse(next);
        }
    }

    NodeType GetNodeType(int layer)
    {
        if (layer == 0)
            return NodeType.Combat; // or NodeType.Start if you have one

        if (layer == layers - 1)
            return NodeType.Boss;

        float r = Random.value;

        if (r < 0.40f) return NodeType.Combat;
        if (r < 0.73f) return NodeType.Event;
        if (r < 0.83f) return NodeType.Treasure;
        if (r < 0.95f) return NodeType.Outpost;
        return NodeType.Special;
    }

    void AssignTypes()
    {
        foreach (var node in reachableNodes)
        {
            node.type = GetNodeType(node.layer);
        }
    }

    void SpawnNodes()
    {
        foreach (var node in reachableNodes)
        {
            GameObject obj = Instantiate(nodePrefab, node.position, Quaternion.identity, transform);

            NodeView view = obj.GetComponent<NodeView>();
            if (view != null)
            {
                view.Initialize(node);
            }
        }
    }

    void DrawConnections()
    {
        HashSet<string> drawn = new HashSet<string>();

        foreach (var node in reachableNodes)
        {
            foreach (var next in node.connections)
            {
                if (!reachableNodes.Contains(next))
                    continue;

                string key = $"{node.layer}_{node.index}->{next.layer}_{next.index}";
                if (drawn.Contains(key))
                    continue;

                drawn.Add(key);

                GameObject lineObj = new GameObject($"Line_{key}");
                lineObj.transform.SetParent(transform);

                LineRenderer line = lineObj.AddComponent<LineRenderer>();
                line.positionCount = 2;
                line.SetPosition(0, node.position);
                line.SetPosition(1, next.position);

                line.startWidth = 0.05f;
                line.endWidth = 0.05f;
                line.useWorldSpace = true;

                if (lineMaterial != null)
                    line.material = lineMaterial;
            }
        }
    }
}