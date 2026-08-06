using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventChoice
{
    [Tooltip("Text to be shown describing what the choice is")]
    public string choiceText;
    [Tooltip("Text to be shown describing what the outcome is")]
    public string outcomeText;
    [Tooltip("List of effects that happen when you select this outcome")]
    public List<EventEffectData> effects = new List<EventEffectData>();
    [Tooltip("All requirements must pass before this choice can be selected.")]
    public List<EventChoiceRequirement> requirements = new List<EventChoiceRequirement>();

    public bool RequirementsMet(RunManager runManager, out string failureReason)
    {
        List<string> failures = new List<string>();

        if (requirements != null)
        {
            foreach (EventChoiceRequirement requirement in requirements)
            {
                if (requirement == null)
                {
                    failures.Add("Missing requirement configuration");
                    continue;
                }

                if (!requirement.IsMet(runManager, out string reason))
                    failures.Add(reason);
            }
        }

        failureReason = string.Join("; ", failures);
        return failures.Count == 0;
    }
}
