using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Rooms/Deck Room")]
public class DeckRoom : Room
{
    [Header("Boarder Stun")]
    [SerializeField, Min(0f)] private float boarderEntryStunDuration = 5f;

    public override void OnUpgrade(RoomInstance instance)
    {
    }

    public float GetBoarderEntryStunDuration(int level)
    {
        if (level >= 1)
        {
            return boarderEntryStunDuration;
        }

        return 0f;
    }
}
