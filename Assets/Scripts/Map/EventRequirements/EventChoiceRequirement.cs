using UnityEngine;

public abstract class EventChoiceRequirement : ScriptableObject
{
    [TextArea] public string customFailureText;

    public abstract bool IsMet(RunManager runManager, out string failureReason);

    protected string FailureOrDefault(string defaultText)
    {
        return string.IsNullOrWhiteSpace(customFailureText)
            ? defaultText
            : customFailureText.Trim();
    }
}
