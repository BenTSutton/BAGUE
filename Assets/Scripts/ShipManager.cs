using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipManager : MonoBehaviour
{
    public List<Room> rooms = new List<Room>();
    public List<Room> allRooms = new List<Room>();

    private Dictionary<string, Room> roomDict = new Dictionary<string, Room>();

    void Awake()
    {
        foreach (var room in allRooms)
        {
            roomDict[room.name] = room;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Y))
        {
            int index = Random.Range(0, allRooms.Count);
            AddRoom(allRooms[index].name);
        }
    }

    private void AddRoom(string roomToAdd)
    {
        Room prefab = roomDict[roomToAdd];

        Room room = Instantiate(prefab);
        rooms.Add(room);
    }
}
