using UnityEngine;

public enum EventResourceType
{
    Credits,
    Scrap,
    Fuel
}

[CreateAssetMenu(menuName = "Map/Event Requirements/Resource")]
public class ResourceEventRequirement : EventChoiceRequirement
{
    public EventResourceType resource;
    [Min(0)] public int minimumAmount = 1;

    public override bool IsMet(RunManager runManager, out string failureReason)
    {
        int currentAmount = runManager == null
            ? 0
            : resource switch
            {
                EventResourceType.Credits => runManager.money,
                EventResourceType.Scrap => runManager.scrap,
                EventResourceType.Fuel => runManager.fuel,
                _ => 0
            };

        failureReason = FailureOrDefault($"Requires {minimumAmount} {resource}");
        return runManager != null && currentAmount >= minimumAmount;
    }
}
