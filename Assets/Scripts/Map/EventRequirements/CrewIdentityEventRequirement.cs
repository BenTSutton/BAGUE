using UnityEngine;

[CreateAssetMenu(menuName = "Map/Event Requirements/Crew Identity")]
public class CrewIdentityEventRequirement : EventChoiceRequirement
{
    public CrewMember requiredCrew;

    public override bool IsMet(RunManager runManager, out string failureReason)
    {
        string crewName = requiredCrew != null ? requiredCrew.crewName : "configured crew";
        failureReason = FailureOrDefault($"Requires {crewName}");
        return runManager != null
            && requiredCrew != null
            && runManager.activeCrew != null
            && runManager.activeCrew.Contains(requiredCrew);
    }
}
