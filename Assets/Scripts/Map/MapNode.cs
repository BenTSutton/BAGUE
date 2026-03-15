using UnityEngine;
using System.Collections.Generic;
public enum NodeType
{
    Combat,
    Event,
    Outpost,
    Treasure,
    Boss,
    Special
}

public class MapNode
{
    public int layer;
    public int index;
    public NodeType type;
    public Vector2 position;
    public List<MapNode> connections = new List<MapNode>();
    public bool visited = false;
}
