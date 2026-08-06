using UnityEngine;

[CreateAssetMenu(menuName = "Map/Event Requirements/Room Level")]
public class RoomLevelEventRequirement : EventChoiceRequirement
{
    public Room requiredRoom;
    [Min(0)] public int minimumLevel = 1;

    public override bool IsMet(RunManager runManager, out string failureReason)
    {
        RoomInstance room = runManager != null && requiredRoom != null
            ? runManager.GetRoomInstance(requiredRoom)
            : null;
        string roomName = requiredRoom != null ? requiredRoom.roomName : "configured room";

        failureReason = FailureOrDefault($"Requires {roomName} level {minimumLevel}");
        return room != null && room.unlocked && room.level >= minimumLevel;
    }
}
