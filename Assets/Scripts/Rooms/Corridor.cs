using UnityEngine;

public class Corridor : Room
{
    public override void OnEnter()
    {
        Debug.Log("Entered Corridor");
    }
}
