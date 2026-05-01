using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//This class is the single source of truth for the state of the run, holds all states for all nodes and has logic to advance the run!
public class MapRunState : MonoBehaviour
{
    public static MapRunState Instance;

    //All nodes have a nodestate which has info on whether it is visited, unlocked, visitable etc, this maps them to each other
    public Dictionary<MapNode, NodeState> states = new Dictionary<MapNode, NodeState>();

    public MapNode currentNode;
    //Database with all the node definitions, when you select a node it will check this database and pull a random definition for the selected node
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
        if(GameManager.Instance.debug)
        {
            GameManager.Instance.gameObject.GetComponent<MapGenerator>().GenerateMap();
        }
    }

    //Reset the current map, all nodes set to default states
    public void Initialize(IEnumerable<MapNode> allNodes, IEnumerable<MapNode> startNodes)
    {
        states.Clear();

        foreach (var node in allNodes)
        {
            states[node] = new NodeState
            {
                node = node,
                discovered = startNodes.Contains(node),
                selectable = startNodes.Contains(node),
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

    //Enter the node!
    public void EnterNode(MapNode node)
    {
        NodeState state = states[node];

        //Check the palyer should be able to go into it
        if (!state.selectable || state.permanentlyLocked || state.completed)
            return;

        //Set this node to active and visited
        currentNode = node;
        state.visited = true;

        //Gets the actual content for the node
        NodeContentDefinition definition = GetOrAssignContent(node, state);

        //If this is an event, open the event dialog panel so the user can proceed
        if (node.type == NodeType.Event && definition is EventDefinition eventDefinition)
        {
            EventDialogPanel.Instance.Open(eventDefinition, state);
            return;
        }
        else if (node.type == NodeType.Special && definition is SpecialDefinition specialDefinition)
        {
            EventDialogPanel.Instance.Open(specialDefinition, state);
            return;
        }

        if (node.type == NodeType.Combat && definition is CombatDefinition combatDefinition)
        {
            GameManager.Instance.EnterCombat(combatDefinition);
            return;
        }

        if (node.type == NodeType.Outpost && definition is OutpostDefinition outpostDefinition)
        {
            OutpostDialogPanel.Instance.Open(outpostDefinition, state);
            return;
            
        }

        if (node.type == NodeType.Treasure && definition is TreasureDefinition treasureDefinition)
        {
            Treasure treasure1 = treasureDatabase.GetRandomTreasure();
            Treasure treasure2 = treasureDatabase.GetRandomTreasure();
            Treasure treasure3 = treasureDatabase.GetRandomTreasure();
            TreasureDialogPanel.Instance.Open(treasureDefinition, state, treasure1, treasure2, treasure3);
            return;
        }

        if (node.type == NodeType.Boss && definition is BossDefinition bossDefinition)
        {
            // Boss
            GameManager.Instance.EnterCombat(bossDefinition);
            return;
        }

        //TBD!! **** LOGIC FOR ALL NODES SHOULD GO HERE, E.G TREASURE SHOULD TRIGGER, COMBAT SHOULD OPEN SCENE ETC **** 
        // Non-event nodes complete immediately
        state.completed = true;
        if (definition != null)
        {
            NodeResolutionResult result = definition.Resolve(state);
            state.resultSummary = result.summary;
        }

        AdvanceFromNode(node);
    }

    //To be called after event completes, to complete the event
    public void CompleteCurrentNodeAfterEvent(MapNode node)
    {
        NodeState state = states[node];
        state.completed = true;
        AdvanceFromNode(node);
    }

    //Advance from the node, 
    void AdvanceFromNode(MapNode chosenNode)
    {
        //Debug.Log("Should advance node");
        //All nodes to not selectable
        foreach (var node in states)
        {
            node.Value.selectable = false;
        }

        //Correctly set the node states 
        foreach (var next in chosenNode.connections)
        {
            //Debug.Log("Should set a node to discovered and selectable");
            NodeState nextState = states[next];
            nextState.discovered = true;
            nextState.selectable = true;
        }

        //LOCK unreachable!
        LockUnreachableNodesFrom(chosenNode);
        NodeMenuPanel.Instance.RefreshAllNodeViews();
    }

    //Get the content for the node from the database
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

    //Lock unreachable nodes so they appear visually not selectable
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

    //Get all reachable nodes, the reachable set is passed through
    void CollectReachable(MapNode node, HashSet<MapNode> visited)
    {
        if (!visited.Add(node))
            return;

        foreach (var next in node.connections)
        {
            CollectReachable(next, visited);
        }
    }
}