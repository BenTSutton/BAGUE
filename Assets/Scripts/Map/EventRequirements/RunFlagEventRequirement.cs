using UnityEngine;

[CreateAssetMenu(menuName = "Map/Event Requirements/Prior Run Flag")]
public class RunFlagEventRequirement : EventChoiceRequirement
{
    public string flagId;
    public bool mustBeSet = true;

    public override bool IsMet(RunManager runManager, out string failureReason)
    {
        bool isSet = runManager != null && runManager.HasRunFlag(flagId);
        failureReason = FailureOrDefault(
            mustBeSet
                ? $"Requires prior flag '{flagId}'"
                : $"Requires flag '{flagId}' to be unset");
        return !string.IsNullOrWhiteSpace(flagId) && isSet == mustBeSet;
    }
}
