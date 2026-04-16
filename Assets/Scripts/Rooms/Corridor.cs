using UnityEngine;

public class Corridor : RoomCollide
{
    public override void OnEnter()
    {
        Debug.Log("Entered Corridor");
    }
}
