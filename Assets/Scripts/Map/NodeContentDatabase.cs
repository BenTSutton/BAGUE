using System.Collections.Generic;
using System.Linq;
using UnityEngine;


//Database of node content, make sure this is updated for all new nodes created
[CreateAssetMenu(menuName = "Map/Node Content Database")]
public class NodeContentDatabase : ScriptableObject
{
    //Faction specific
    public List<NodeContentDefinition> fishCombatDefinitions;
    
    //Generic combats - WIP
    public List<NodeContentDefinition> combatDefinitions;
    public List<NodeContentDefinition> eventDefinitions;
    public List<NodeContentDefinition> treasureDefinitions;
    public List<NodeContentDefinition> outpostDefinitions;
    public List<NodeContentDefinition> specialDefinitions;
    public List<NodeContentDefinition> bossDefinitions;

    //Get a random node from the database for the specified type
    public NodeContentDefinition GetRandomForType(NodeType type)
    {

        List<NodeContentDefinition> pool = new List<NodeContentDefinition>();

        if(type == NodeType.Combat)
        {
            pool = GetCombatDefinitionPoolForEnemyType();
        }
        else
        {
            pool = type switch
            {
                NodeType.Event => eventDefinitions,
                NodeType.Treasure => treasureDefinitions,
                NodeType.Outpost => outpostDefinitions,
                NodeType.Special => specialDefinitions,
                NodeType.Boss => bossDefinitions,
                _ => null
            };
        }

        if (pool == null || pool.Count == 0)
            return null;

        //Need the seed so its actually random every time
        Random.seed = System.DateTime.Now.Millisecond;
        return pool[Random.Range(0, pool.Count)];
    }

    List<NodeContentDefinition> GetCombatDefinitionPoolForEnemyType()
    {
        switch(RunManager.Instance.enemyFaction.FactionName)
        {
            case "Star Fish":
                return fishCombatDefinitions;
        }
        return combatDefinitions;
    }

    //Match the definition by ID
    public NodeContentDefinition GetById(string id)
    {
        return combatDefinitions
            .Concat(eventDefinitions)
            .Concat(treasureDefinitions)
            .Concat(outpostDefinitions)
            .Concat(specialDefinitions)
            .FirstOrDefault(x => x.id == id);
    }
}