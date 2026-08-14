using UnityEngine;

[CreateAssetMenu(menuName = "Map/Event Requirements/Hull Percentage")]
public class HullPercentageEventRequirement : EventChoiceRequirement
{
    [Range(0f, 1f)] public float minimumHullPercentage = 0.5f;

    public override bool IsMet(RunManager runManager, out string failureReason)
    {
        float percentage = runManager != null && runManager.maxShipHealth > 0
            ? (float)runManager.currentShipHealth / runManager.maxShipHealth
            : 0f;
        int displayPercentage = Mathf.RoundToInt(minimumHullPercentage * 100f);

        failureReason = FailureOrDefault($"Requires at least {displayPercentage}% hull");
        return runManager != null && percentage >= minimumHullPercentage;
    }
}
