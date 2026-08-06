using UnityEngine;

[CreateAssetMenu(menuName = "Map/Event Requirements/Crew Category")]
public class CrewCategoryEventRequirement : EventChoiceRequirement
{
    public CrewRecruitmentCategory requiredCategory;
    [Min(1)] public int minimumCrew = 1;

    public override bool IsMet(RunManager runManager, out string failureReason)
    {
        int count = 0;
        if (runManager != null && runManager.activeCrew != null)
        {
            count = runManager.activeCrew.FindAll(crew =>
                crew != null && crew.recruitmentCategory == requiredCategory).Count;
        }

        failureReason = FailureOrDefault(
            $"Requires {minimumCrew} {requiredCategory} crew");
        return count >= minimumCrew;
    }
}
