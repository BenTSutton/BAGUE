using UnityEngine;
using System.Collections.Generic;
using System.Linq;

//This class has comlpex logic to generate the Map, with multiple paths and different types of Nodes
public class MapGenerator : MonoBehaviour
{
    [Header("Map Size")]
    //How tall the map should be, e.g how many rows - REMEMBER THESE VARIABLES ARE CHANGED FROM THE INSPECTOR, NOT THE VALUES SHOWN HERE
    public int layers = 10;
    //How wide it should be
    public int width = 4;
    //How many starter nodes there should be
    public int pathCount = 6;

    [Header("Layout")]
    //Spacing between the nodes
    public float xSpacing = 2f;
    public float ySpacing = 1.5f;

    [Header("Prefabs")]
    public GameObject nodePrefab;
    public GameObject shipPrefab;
    //Material for the lines between nodes
    public Material lineMaterial;

    //2d grid with all the nodes
    private List<List<MapNode>> map = new List<List<MapNode>>();
    //2d grid with only nodes on a path
    private HashSet<MapNode> reachableNodes = new HashSet<MapNode>();
    //2d grid with only the starting nodes
    private List<MapNode> startNodes = new List<MapNode>();
    //The big boss node
    private MapNode bossNode;

    //All the logic to generate a new map, including clearing the old one
    public void GenerateMap()
    {
        ClearOldMap();
        CreateGrid();
        GeneratePaths();
        AddExtraBranches();
        FindReachableNodes();
        AssignTypes();
        //Initialize the nodes, sets values such as "visited" to false" so you everything starts unexplored
        MapRunState.Instance.Initialize(reachableNodes, startNodes);
        SpawnNodes();
        DrawConnections();
        SpawnShip();
    }

    //Clears all references to the previous map, to allow the generator to start anew
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

    //Makes the grid, filling in all spaces in the grid.  No pathing in play yet
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
                //Sets position of node centred on x = 0, and y = the layer 
                node.position = new Vector2((x - (width - 1) / 2f) * xSpacing, y * ySpacing);

                row.Add(node);
            }

            map.Add(row);
        }
    }

    // Makes the paths out of all the available nodes
    void GeneratePaths()
    {
        // Pick unique starting columns
        List<int> startColumns = PickStartColumns(Mathf.Min(pathCount, width));

        foreach (int col in startColumns)
        {
            startNodes.Add(map[0][col]);
        }

        //Set the BOSS NODE.
        int bossX = width / 2;
        bossNode = map[layers - 1][bossX];

        for (int p = 0; p < pathCount; p++)
        {
            int currentX = startColumns[p % startColumns.Count];

            // Stop one layer earlier so the final path is always to the BOSS
            for (int y = 0; y < layers - 2; y++)
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

            // Force final path into the boss node
            MapNode preBoss = map[layers - 2][currentX];

            if (!preBoss.connections.Contains(bossNode))
            {
                preBoss.connections.Add(bossNode);
            }
        }

        //Some housekeeping
        EnsureAllStartsHaveConnection();
    }

    //Select starting nodes
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

    //Either left, right, or straight
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

    //Choose the next node to move to 
    int ChooseBestNextX(int layer, int currentX, List<int> candidates)
    {
        // Prefer straight, then slight movement, and avoid crossing over
        List<int> valid = new List<int>();

        foreach (int nextX in candidates)
        {
            if (!WouldCrossExistingConnection(layer, currentX, nextX))
            {
                valid.Add(nextX);
            }
        }

        // If everything crosses, then all nodes are valid, so generation of the paths can continue
        if (valid.Count == 0)
        {
            valid = candidates;
        }

        // Nodes are weighted so straight paths preferred over diagonal
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

    //This stops dead ends and makes the paths more interesting rather than just being straight paths
    void AddExtraBranches()
    {
        for (int y = 0; y < layers - 1; y++)
        {
            for (int x = 0; x < width; x++)
            {
                MapNode node = map[y][x];

                if (node.connections.Count == 0 && !startNodes.Contains(node))
                    continue;

                if (Random.value > 0.45f)
                    continue;

                List<int> candidates = GetCandidateNextXs(x)
                    .Where(nextX =>
                    {
                        MapNode target = map[y + 1][nextX];

                        if (node.connections.Contains(target))
                            return false;

                        // On the final layer, only allow the BOSS
                        if (target.layer == layers - 1 && target != bossNode)
                            return false;

                        return CanReachBoss(target);
                    })
                    .ToList();

                foreach (int nextX in candidates.OrderBy(_ => Random.value))
                {
                    if (WouldCrossExistingConnection(y, x, nextX))
                        continue;

                    node.connections.Add(map[y + 1][nextX]);
                    break;
                }
            }
        }
    }

    //Checks if a node can reach the boss via any path
    bool CanReachBoss(MapNode node, HashSet<MapNode> visited = null)
    {
        if (node == bossNode)
            return true;

        //This sets visited to a new hashset if it is null, to allow the recursion
        visited ??= new HashSet<MapNode>();

        if (!visited.Add(node))
            return false;

        foreach (var next in node.connections)
        {
            if (CanReachBoss(next, visited))
                return true;
        }

        return false;
    }

    //Check if drawing a connection between two nodes would cross an existing connection, not visually appealing to do so
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

    //Check every start node has a connected node to move to 
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

    //Get all nodes reachable from a start node
    void FindReachableNodes()
    {
        reachableNodes.Clear();

        foreach (var start in startNodes)
        {
            Traverse(start);
        }
    }

    //Recursively traverse up a nodes connections to find all on its path
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

    //Gets a random node type depending on some weighting, this defines what type a node will be.
    NodeType GetNodeType(MapNode node)
    {
        if (node.layer == 0)
            return NodeType.Combat;

        if (node == bossNode)
            return NodeType.Boss;

        float r = Random.value;

        if (r < 0.40f) return NodeType.Combat;
        if (r < 0.73f) return NodeType.Event;
        if (r < 0.83f) return NodeType.Treasure;
        if (r < 0.95f) return NodeType.Outpost;
        return NodeType.Special;
    }

    //Make sure all nodes have a type
    void AssignTypes()
    {
        foreach (var node in reachableNodes)
        {
            node.type = GetNodeType(node);
        }
    }

    //Spawn the nodes in the scene
    void SpawnNodes()
    {
        foreach (var node in reachableNodes)
        {
            GameObject obj = Instantiate(nodePrefab, node.position, Quaternion.identity, transform);

            //Initialize the nodes view, to make sure it appears correctly in the scene
            NodeView view = obj.GetComponent<NodeView>();
            if (view != null)
            {
                view.Initialize(node);
            }
        }
    }

    //Draw the connections between the nodes
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

    //Spawn ship on map 
    void SpawnShip()
    {
        Vector2 spawnPosition = startNodes[startNodes.Count / 2].position;
        spawnPosition = new Vector2(0, spawnPosition.y - (ySpacing * 2));
        GameObject.Instantiate(shipPrefab, spawnPosition, Quaternion.identity, transform);
    }
}