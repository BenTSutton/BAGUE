using System.Collections.Generic;
using UnityEngine;

/* This class is used to create an EventDefinition, which is the asset necessary to define a new Event.
If you want to create an Event, create this in the Editor, and assign the relevant areas.
*/

[CreateAssetMenu(menuName = "Map/Node Content/Event")]
public class EventDefinition : NodeContentDefinition
{
    [Tooltip("Intro text shown when you have selected the Event node")]
    [TextArea] public string introText;
    [Tooltip("Choices to be added, there should be 2 Choices added currently")]
    public List<EventChoice> choices = new List<EventChoice>();

    public override NodeResolutionResult Resolve(NodeState state)
    {
        return new NodeResolutionResult
        {
            title = displayName,
            summary = state.resultSummary
        };
    }

    public NodeResolutionResult ResolveChoice(NodeState state, int optionIndex)
    {
        //Make sure the optionIndex is a valid, should be either 0 or 1 for 2 choices
        if (optionIndex < 0 || optionIndex >= choices.Count)
        {
            return new NodeResolutionResult
            {
                title = displayName,
                summary = "Invalid choice, please pick again!"
            };
        }

        EventChoice choice = choices[optionIndex];

        state.eventChoiceMade = true;
        state.chosenOptionIndex = optionIndex;
        state.resultSummary = choice.outcomeText;

        ApplyChoiceEffects(choice);

        return new NodeResolutionResult
        {
            title = displayName,
            description = introText,
            summary = choice.outcomeText
        };
    }

    void ApplyChoiceEffects(EventChoice choice)
    {
        foreach (var effect in choice.effects)
        {
            ApplyEffect(effect);
        }
    }

    //Actually apply the effects of the event, logic should go here for all
    void ApplyEffect(EventEffectData effect)
    {
        switch (effect.effectType)
        {
            case EventEffectType.HealShip:
                RunManager.Instance.AddHealth(effect.amount);
                break;

            case EventEffectType.DamageShip:
                RunManager.Instance.DamageShip(effect.amount);
                break;

            case EventEffectType.AddMoney:
                RunManager.Instance.AddMoney(effect.amount);
                break;

            case EventEffectType.AddFuel:
                RunManager.Instance.AddFuel(effect.amount);
                break;
            case EventEffectType.AddCrew:
                RunManager.Instance.AddCrew(effect.crewName);
                break;
        }
    }
}