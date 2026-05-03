using UnityEngine;

public class RoomCollider : MonoBehaviour
{
    public RoomCollide room;

    void OnTriggerEnter2D(Collider2D col)
    {
        //Debug.Log(col.gameObject.name + " : " + gameObject.name + " : " + Time.time);
        if(col.gameObject.name == "Player")
        {
            room.OnEnter();
        }
    }
}
