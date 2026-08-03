using System.Collections.Generic;
using System.Linq;
using UnityEngine;


//Database of node content, make sure this is updated for all new nodes created
[CreateAssetMenu(menuName = "Crew/Crew Database")]
public class CrewDatabase : ScriptableObject
{
    public List<CrewMember> crewMembers;

    //Get a random node from the database for the specified type
    /*public NodeContentDefinition GetRandomForType(NodeType type)
    {
        List<NodeContentDefinition> pool = type switch
        {
            NodeType.Combat => combatDefinitions,
            NodeType.Event => eventDefinitions,
            NodeType.Treasure => treasureDefinitions,
            NodeType.Outpost => outpostDefinitions,
            NodeType.Special => specialDefinitions,
            _ => null
        };

        if (pool == null || pool.Count == 0)
            return null;

        //Need the seed so its actually random every time
        Random.seed = System.DateTime.Now.Millisecond;
        return pool[Random.Range(0, pool.Count)];
    }*/

    //Match the definition by ID
    public CrewMember GetByName(string name)
    {
        return crewMembers
            .FirstOrDefault(x => x.crewName == name);
    }

    public CrewMember GetRandomCrew()
    {
        if (crewMembers == null || crewMembers.Count == 0)
            return null;

        return crewMembers[Random.Range(0, crewMembers.Count)];
    }

    public CrewMember GetRandomPurchasableCrew(IEnumerable<CrewMember> excludedCrew)
    {
        HashSet<CrewMember> excluded = excludedCrew != null
            ? new HashSet<CrewMember>(excludedCrew)
            : new HashSet<CrewMember>();

        List<CrewMember> pool = crewMembers
            .Where(crew => crew != null && crew.purchasable && !excluded.Contains(crew))
            .ToList();

        if (pool.Count == 0)
            return null;

        return pool[Random.Range(0, pool.Count)];
    }
}
