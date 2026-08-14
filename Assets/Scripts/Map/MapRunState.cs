using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Owns map progress for the current run: node state, travel, content and route advancement.
public class MapRunState : MonoBehaviour
{
    public static MapRunState Instance;

    public Dictionary<MapNode, NodeState> states = new Dictionary<MapNode, NodeState>();

    public MapNode currentNode;
    public NodeContentDatabase contentDatabase;
    public TreasureDatabase treasureDatabase;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        if (GameManager.Instance.debug && SceneManager.GetActiveScene().name != "Menu")
            GameManager.Instance.gameObject.GetComponent<MapGenerator>().GenerateMap();
    }

    public void Initialize(IEnumerable<MapNode> allNodes, IEnumerable<MapNode> startNodes)
    {
        states.Clear();
        HashSet<MapNode> startingNodes = new HashSet<MapNode>(startNodes);

        foreach (MapNode node in allNodes)
        {
            states[node] = new NodeState
            {
                node = node,
                discovered = startingNodes.Contains(node),
                selectable = startingNodes.Contains(node),
                visited = false,
                completed = false,
                permanentlyLocked = false
            };
        }

        currentNode = null;
    }

    public NodeState GetState(MapNode node)
    {
        return states[node];
    }

    public bool EnterNode(MapNode node)
    {
        if (node == null || !states.TryGetValue(node, out NodeState state))
            return false;

        if (!state.selectable || state.permanentlyLocked || state.completed)
            return false;

        int fuelCost = GetTravelFuelCost(node);
        if (RunManager.Instance == null || RunManager.Instance.fuel < fuelCost)
        {
            Debug.Log($"Not enough fuel to enter this node. Requires {fuelCost} fuel.");
            return false;
        }

        MarkTravelRouteCompleted(node);

        currentNode = node;
        state.visited = true;

        if (fuelCost > 0)
            RunManager.Instance.RemoveFuel(fuelCost);

        NodeContentDefinition definition = GetOrAssignContent(node, state);
        if (OpenNodeContent(definition, state))
            return true;

        // Definitions without a dedicated panel or scene resolve immediately.
        state.completed = true;
        if (definition != null)
        {
            NodeResolutionResult result = definition.Resolve(state);
            state.resultSummary = result.summary;
        }

        AdvanceFromNode(node);
        return true;
    }

    private void MarkTravelRouteCompleted(MapNode destination)
    {
        if (currentNode == null)
            return;

        string routeKey = GetNodeKey(currentNode, destination);
        MapGenerator mapGenerator = GameManager.Instance.GetComponent<MapGenerator>();
        if (mapGenerator.routeLines.TryGetValue(routeKey, out MapRouteLine routeLine))
            routeLine.MarkCompleted();
    }

    private bool OpenNodeContent(NodeContentDefinition definition, NodeState state)
    {
        switch (definition)
        {
            case BossDefinition boss:
                RunManager.Instance.inBossFight = true;
                GameManager.Instance.EnterCombat(boss);
                return true;

            case EventDefinition eventDefinition:
                EventDialogPanel.Instance.Open(eventDefinition, state);
                return true;

            case OutpostDefinition outpost:
                OutpostDialogPanel.Instance.Open(outpost, state);
                return true;

            case TreasureDefinition treasureDefinition:
                OpenTreasureNode(treasureDefinition, state);
                return true;

            case CombatDefinition combat:
                GameManager.Instance.EnterCombat(combat);
                return true;

            default:
                return false;
        }
    }

    private void OpenTreasureNode(TreasureDefinition definition, NodeState state)
    {
        Treasure first = treasureDatabase.GetRandomTreasure();
        Treasure second = treasureDatabase.GetRandomTreasure(new[] { first });
        Treasure third = treasureDatabase.GetRandomTreasure(new[] { first, second });
        TreasureDialogPanel.Instance.Open(definition, state, first, second, third);
    }

    public int GetTravelFuelCost(MapNode node)
    {
        if (node == null || currentNode == null || RunManager.Instance == null)
            return 0;

        return RunManager.Instance.GetJumpFuelCost();
    }

    public bool CanAffordTravelTo(MapNode node)
    {
        return RunManager.Instance != null
            && RunManager.Instance.fuel >= GetTravelFuelCost(node);
    }

    public bool CompleteCombatNode(CombatDefinition combatDefinition)
    {
        if (currentNode == null || !states.TryGetValue(currentNode, out NodeState state))
            return false;

        if (state.completed)
            return false;

        if (!state.combatRewardsGranted && combatDefinition != null)
        {
            state.combatRewardsGranted = true;
            combatDefinition.GrantVictoryRewards(RunManager.Instance);
        }

        state.completed = true;
        AdvanceFromNode(currentNode);
        return true;
    }

    public bool CompleteCombatNodeWithoutRewards(string summary)
    {
        if (currentNode == null || !states.TryGetValue(currentNode, out NodeState state))
        {
            return false;
        }

        if (state.completed)
            return false;

        // Prevent any later path from granting this node's rewards.
        state.combatRewardsGranted = true;
        state.completed = true;
        state.resultSummary = summary;

        AdvanceFromNode(currentNode);
        return true;
    }

    public void CompleteCurrentNodeAfterEvent(MapNode node)
    {
        if (node == null || !states.TryGetValue(node, out NodeState state) || state.completed)
            return;

        state.completed = true;
        AdvanceFromNode(node);
    }

    void AdvanceFromNode(MapNode chosenNode)
    {
        foreach (NodeState state in states.Values)
            state.selectable = false;

        foreach (MapNode next in chosenNode.connections)
        {
            NodeState nextState = states[next];
            nextState.discovered = true;
            nextState.selectable = true;
        }

        LockUnreachableNodesFrom(chosenNode);
        GameManager.Instance.gameObject.GetComponent<MapGenerator>().RefreshRouteAccessibility(states);
        NodeMenuPanel.Instance.RefreshAllNodeViews(false);
        TarotManager.Instance?.TryOpenTarot();
    }

    public NodeContentDefinition GetOrAssignContent(MapNode node, NodeState state)
    {
        if (!string.IsNullOrEmpty(state.generatedContentId))
        {
            return contentDatabase.GetById(state.generatedContentId);
        }

        NodeContentDefinition definition = contentDatabase.GetRandomForType(node.type);

        if (definition != null)
        {
            state.generatedContentId = definition.id;
        }

        return definition;
    }

    void LockUnreachableNodesFrom(MapNode chosenNode)
    {
        HashSet<MapNode> reachable = new HashSet<MapNode>();
        CollectReachable(chosenNode, reachable);

        foreach (var kvp in states)
        {
            MapNode node = kvp.Key;
            NodeState state = kvp.Value;

            if (node == chosenNode)
                continue;

            // Any node not reachable from the chosen node can never be taken now
            if (!reachable.Contains(node) && !state.visited)
            {
                state.permanentlyLocked = true;
                state.selectable = false;
            }
        }
    }

    void CollectReachable(MapNode node, HashSet<MapNode> visited)
    {
        if (!visited.Add(node))
            return;

        foreach (var next in node.connections)
        {
            CollectReachable(next, visited);
        }
    }

    string GetNodeKey(MapNode node1, MapNode node2)
    {
        if (node1.layer > node2.layer)
            (node1, node2) = (node2, node1);

        return $"{node1.layer}_{node1.index}->{node2.layer}_{node2.index}";
    }
}
