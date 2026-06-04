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
        RunManager run = RunManager.Instance;

        switch (effect.effectType)
        {
            case EventEffectType.HealShip:
                run.AddHealth(effect.amount);
                break;

            case EventEffectType.DamageShip:
                run.DamageShip(effect.amount);
                break;

            case EventEffectType.AddFuel:
                run.AddFuel(effect.amount);
                break;

            case EventEffectType.AddMoney:
                run.AddMoney(effect.amount);
                break;

            case EventEffectType.AddScrap:
                run.AddScrap(effect.amount);
                break;

            case EventEffectType.AddCrew:
            case EventEffectType.GainCrew:
                run.AddCrew(effect.crewName);
                break;

            case EventEffectType.GainRandomCrew:
                AddRandomCrew(false);
                break;

            case EventEffectType.GainRandomRareCrew:
                AddRandomCrew(true);
                break;

            case EventEffectType.LoseCrew:
                RemoveRandomCrew();
                break;

            case EventEffectType.MoneyOrDamage:
                if (RollChance(effect.percentageOdds))
                    run.AddMoney(effect.amount);
                else
                    run.DamageShip(effect.secondAmount);
                break;

            case EventEffectType.CreditsOrFuel:
                if (RollChance(effect.percentageOdds))
                    run.AddMoney(effect.amount);
                else
                    run.AddFuel(effect.secondAmount);
                break;

            case EventEffectType.AddOrLoseMoney:
                if (RollChance(effect.percentageOdds))
                    run.AddMoney(effect.amount);
                else
                    run.RemoveMoney(effect.secondAmount);
                break;

            case EventEffectType.GainCrewOrLoseCredits:
                if (RollChance(effect.percentageOdds))
                    run.AddCrew(effect.crewName);
                else
                    run.RemoveMoney(effect.amount);
                break;

            case EventEffectType.DuplicateCrewOrKill:
                if (run.activeCrew.Count == 0)
                    break;

                CrewMember randomCrew = run.activeCrew[UnityEngine.Random.Range(0, run.activeCrew.Count)];

                if (RollChance(effect.percentageOdds))
                    run.activeCrew.Add(randomCrew);
                else
                    run.activeCrew.Remove(randomCrew);

                break;

            case EventEffectType.LoseCrewAndNextFightHas1HP:
                RemoveRandomCrew();

                run.nextFightHasOneHP = true;
                break;

            case EventEffectType.CanSeeCombatsBeforeStarting:
                run.canSeeCombatsBeforeStarting = true;
                break;

            case EventEffectType.UnlockEvent:
                //TODO
                break;

            case EventEffectType.RandomEffect:
                ApplyRandomSimpleEffect();
                break;
            case EventEffectType.GainOrLoseCredits:
                if (RollChance(effect.percentageOdds))
                    run.AddMoney(effect.amount);
                else
                    run.AddMoney(effect.secondAmount);
                break;
        }
    }

    void RemoveRandomCrew()
    {
        RunManager run = RunManager.Instance;

        if (run.activeCrew.Count == 0)
            return;

        int index = UnityEngine.Random.Range(0, run.activeCrew.Count);
        run.activeCrew.RemoveAt(index);
    }

    void AddRandomCrew(bool rareOnly)
    {
        RunManager run = RunManager.Instance;

        CrewMember crew = run.crewDatabase.GetRandomCrew();

        if (crew == null)
            return;

        if (!run.activeCrew.Contains(crew))
            run.activeCrew.Add(crew);
    }

    void ApplyRandomSimpleEffect()
    {
        RunManager run = RunManager.Instance;

        int roll = UnityEngine.Random.Range(0, 7);

        switch (roll)
        {
            case 0:
                run.AddMoney(20);
                break;
            case 1:
                run.RemoveMoney(20);
                break;
            case 2:
                run.AddFuel(10);
                break;
            case 3:
                run.RemoveFuel(10);
                break;
            case 4:
                run.AddHealth(20);
                break;
            case 5:
                run.DamageShip(15);
                break;
            case 6:
                run.AddScrap(10);
                break;
        }
    }

    bool RollChance(int percent)
    {
        return UnityEngine.Random.Range(1, 101) <= percent;
    }
}