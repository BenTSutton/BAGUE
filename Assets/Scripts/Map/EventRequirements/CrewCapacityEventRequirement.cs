using UnityEngine;

[CreateAssetMenu(menuName = "Map/Event Requirements/Crew Capacity")]
public class CrewCapacityEventRequirement : EventChoiceRequirement
{
    [Min(1)] public int minimumOpenSlots = 1;

    public override bool IsMet(RunManager runManager, out string failureReason)
    {
        int openSlots = runManager != null ? runManager.AvailableCrewSlots : 0;

        failureReason = FailureOrDefault(
            $"Requires {minimumOpenSlots} open crew slot"
            + (minimumOpenSlots == 1 ? string.Empty : "s"));
        return openSlots >= minimumOpenSlots;
    }
}
