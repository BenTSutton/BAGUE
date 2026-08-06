using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Node Content/Event")]
public class EventDefinition : NodeContentDefinition
{
    [Tooltip("Intro text shown when the player opens this event.")]
    [TextArea] public string introText;
    [Tooltip("Choices shown to the player. Existing UI supports any configured count.")]
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
        if (choices == null || optionIndex < 0 || optionIndex >= choices.Count)
        {
            return new NodeResolutionResult
            {
                title = displayName,
                summary = "Invalid choice, please pick again!"
            };
        }

        EventChoice choice = choices[optionIndex];

        if (choice == null)
        {
            return new NodeResolutionResult
            {
                title = displayName,
                summary = "This event choice is not configured correctly."
            };
        }

        if (!choice.RequirementsMet(RunManager.Instance, out string requirementFailure))
        {
            return new NodeResolutionResult
            {
                title = displayName,
                description = introText,
                summary = $"Choice locked: {requirementFailure}"
            };
        }

        List<string> consequences = ApplyChoiceEffects(choice);
        string resolvedSummary = BuildResolvedSummary(choice.outcomeText, consequences);

        state.eventChoiceMade = true;
        state.chosenOptionIndex = optionIndex;
        state.resultSummary = resolvedSummary;

        return new NodeResolutionResult
        {
            title = displayName,
            description = introText,
            summary = resolvedSummary
        };
    }

    List<string> ApplyChoiceEffects(EventChoice choice)
    {
        List<string> consequences = new List<string>();

        if (choice == null || choice.effects == null)
            return consequences;

        EffectResult recruitmentCheck = ValidateCrewRecruitment(choice);
        if (!recruitmentCheck.shouldContinue)
        {
            consequences.Add(recruitmentCheck.summary);
            return consequences;
        }

        foreach (var effect in choice.effects)
        {
            EffectResult result = ApplyEffect(effect);

            if (!string.IsNullOrWhiteSpace(result.summary))
                consequences.Add(result.summary);

            if (!result.shouldContinue)
                break;
        }

        return consequences;
    }

    EffectResult ValidateCrewRecruitment(EventChoice choice)
    {
        RunManager run = RunManager.Instance;
        if (run == null || choice == null || choice.effects == null)
            return EffectResult.Continue(string.Empty);

        foreach (EventEffectData effect in choice.effects)
        {
            if (effect == null)
                continue;

            if (effect.effectType == EventEffectType.AddCrew
                || effect.effectType == EventEffectType.GainCrew)
            {
                CrewMember crew = run.crewDatabase != null
                    ? run.crewDatabase.GetByName(effect.crewName)
                    : null;
                CrewAcquisitionResult result = crew != null
                    ? run.GetCrewAcquisitionAvailability(crew)
                    : CrewAcquisitionResult.UnknownCrew;

                if (result != CrewAcquisitionResult.Success)
                    return EffectResult.Stop(run.GetCrewAcquisitionMessage(result, crew));
            }

            if (effect.effectType != EventEffectType.GainRandomCrew
                && effect.effectType != EventEffectType.GainRandomRareCrew)
            {
                continue;
            }

            if (!run.HasCrewCapacity)
                return EffectResult.Stop(
                    run.GetCrewAcquisitionMessage(CrewAcquisitionResult.RosterFull));

            if (run.crewDatabase == null)
                return EffectResult.Stop(
                    "No crew database was available, so nobody joined.");

            CrewRecruitmentCategory[] categories =
                effect.effectType == EventEffectType.GainRandomRareCrew
                    ? new[] { CrewRecruitmentCategory.Rare }
                    : new[]
                    {
                        CrewRecruitmentCategory.Common,
                        CrewRecruitmentCategory.Uncommon
                    };

            if (!run.crewDatabase.HasRecruitableCrew(categories, run.activeCrew))
            {
                string category = effect.effectType == EventEffectType.GainRandomRareCrew
                    ? "rare "
                    : string.Empty;
                return EffectResult.Stop(
                    $"No unowned {category}crew members were available to recruit.");
            }
        }

        return EffectResult.Continue(string.Empty);
    }

    EffectResult ApplyEffect(EventEffectData effect)
    {
        if (effect == null)
        {
            Debug.LogWarning($"Event '{displayName}' contains a null effect.", this);
            return EffectResult.Continue("A configured effect was missing, so nothing happened.");
        }

        if (!EventEffectSupport.IsImplemented(effect.effectType))
        {
            Debug.LogWarning(
                $"Event effect '{effect.effectType}' is not implemented. No effect was applied for '{displayName}'.",
                this);
            return EffectResult.Continue($"{effect.effectType} is not implemented, so it had no effect.");
        }

        RunManager run = RunManager.Instance;

        if (run == null)
        {
            Debug.LogError($"Cannot resolve event '{displayName}' because no RunManager exists.", this);
            return EffectResult.Stop("The event could not be resolved because the run state is unavailable.");
        }

        if (run.TryProtectFromEventEffect(effect, out string protectionName))
        {
            return EffectResult.Continue(
                $"{protectionName} protected you from {effect.effectType}.");
        }

        switch (effect.effectType)
        {
            case EventEffectType.HealShip:
                return ApplyHealthChange(() => run.AddHealth(effect.amount));

            case EventEffectType.DamageShip:
                return ApplyHealthChange(() => run.DamageShip(Mathf.Abs(effect.amount)));

            case EventEffectType.AddFuel:
                return ApplyFuelChange(effect.amount);

            case EventEffectType.AddMoney:
                return ApplyCreditChange(effect.amount);

            case EventEffectType.AddScrap:
                return ApplyScrapGain(effect.amount);

            case EventEffectType.AddCrew:
            case EventEffectType.GainCrew:
                return EffectResult.Continue(TryRecruitNamedCrew(effect.crewName));

            case EventEffectType.GainRandomCrew:
                return EffectResult.Continue(AddRandomCrew(false));

            case EventEffectType.GainRandomRareCrew:
                return EffectResult.Continue(AddRandomCrew(true));

            case EventEffectType.LoseCrew:
            {
                string result = RemoveRandomCrew(out bool removedCrew);
                return removedCrew
                    ? EffectResult.Continue(result)
                    : EffectResult.Stop(result);
            }

            case EventEffectType.MoneyOrDamage:
                if (RollChance(effect.percentageOdds))
                    return ApplyCreditChange(Mathf.Abs(effect.amount));
                return ApplyHealthChange(() => run.DamageShip(Mathf.Abs(effect.secondAmount)));

            case EventEffectType.CreditsOrFuel:
                if (RollChance(effect.percentageOdds))
                    return ApplyCreditChange(Mathf.Abs(effect.amount));
                return ApplyFuelChange(Mathf.Abs(effect.secondAmount));

            case EventEffectType.AddOrLoseMoney:
                if (RollChance(effect.percentageOdds))
                    return ApplyCreditChange(Mathf.Abs(effect.amount));
                return ApplyCreditChange(-Mathf.Abs(effect.secondAmount));

            case EventEffectType.GainCrewOrLoseCredits:
                if (RollChance(effect.percentageOdds))
                    return EffectResult.Continue(TryRecruitNamedCrew(effect.crewName));
                return ApplyCreditChange(-Mathf.Abs(effect.amount));

            case EventEffectType.DuplicateCrewOrKill:
            {
                List<CrewMember> validCrew = run.activeCrew.FindAll(crew => crew != null);
                if (validCrew.Count == 0)
                    return EffectResult.Continue("There was no crew member for the machine to affect.");

                CrewMember randomCrew = validCrew[UnityEngine.Random.Range(0, validCrew.Count)];

                if (RollChance(effect.percentageOdds))
                {
                    CrewAcquisitionResult result = run.TryRecruitCrew(randomCrew, true);
                    return EffectResult.Continue(
                        result == CrewAcquisitionResult.Success
                            ? $"The machine cloned {randomCrew.crewName}."
                            : $"The machine could not clone {randomCrew.crewName}. "
                                + run.GetCrewAcquisitionMessage(result, randomCrew));
                }

                run.activeCrew.Remove(randomCrew);
                return EffectResult.Continue($"The machine killed {randomCrew.crewName}.");
            }

            case EventEffectType.LoseCrewAndNextFightHas1HP:
            {
                string crewResult = RemoveRandomCrew(out _);
                run.nextFightHasOneHP = true;
                return EffectResult.Continue($"{crewResult} Your ship will begin the next fight at 1 hull.");
            }

            case EventEffectType.CanSeeCombatsBeforeStarting:
            {
                bool wasAlreadyActive = run.canSeeCombatsBeforeStarting;
                run.canSeeCombatsBeforeStarting = true;
                return EffectResult.Continue(wasAlreadyActive
                    ? "Combat routes were already visible."
                    : "Combat encounters are now revealed before entry.");
            }

            case EventEffectType.RandomEffect:
                return ApplyRandomSimpleEffect();

            case EventEffectType.GainOrLoseCredits:
                if (RollChance(effect.percentageOdds))
                    return ApplyCreditChange(Mathf.Abs(effect.amount));
                return ApplyCreditChange(-Mathf.Abs(effect.secondAmount));

            case EventEffectType.SpendCredits:
            {
                int cost = Mathf.Abs(effect.amount);
                if (!run.TrySpendMoney(cost))
                    return EffectResult.Stop($"You needed {cost} Credits, but could not afford the purchase.");

                return EffectResult.Continue($"Spent {cost} Credits.");
            }

            case EventEffectType.SetRunFlag:
            {
                bool newlySet = run.SetRunFlag(effect.flagId);
                return EffectResult.Continue(newlySet
                    ? $"Run flag '{effect.flagId}' was recorded."
                    : $"Run flag '{effect.flagId}' was already recorded.");
            }

            case EventEffectType.None:
                return EffectResult.Continue(string.Empty);
        }

        return EffectResult.Continue(string.Empty);
    }

    string RemoveRandomCrew(out bool removedCrew)
    {
        RunManager run = RunManager.Instance;
        run.activeCrew.RemoveAll(crew => crew == null);

        if (run.activeCrew.Count == 0)
        {
            removedCrew = false;
            return "There was no crew member to lose.";
        }

        int index = UnityEngine.Random.Range(0, run.activeCrew.Count);
        string crewName = run.activeCrew[index].crewName;
        run.activeCrew.RemoveAt(index);
        removedCrew = true;
        return $"{crewName} was lost.";
    }

    string TryRecruitNamedCrew(string crewName)
    {
        RunManager run = RunManager.Instance;
        CrewMember crew = run.crewDatabase != null
            ? run.crewDatabase.GetByName(crewName)
            : null;
        CrewAcquisitionResult result = run.TryRecruitCrew(crewName);
        return run.GetCrewAcquisitionMessage(result, crew);
    }

    string AddRandomCrew(bool rareOnly)
    {
        RunManager run = RunManager.Instance;

        if (!run.HasCrewCapacity)
            return run.GetCrewAcquisitionMessage(CrewAcquisitionResult.RosterFull);

        if (run.crewDatabase == null)
        {
            Debug.LogError("Cannot add random crew because RunManager has no CrewDatabase assigned.", this);
            return "No crew database was available, so nobody joined.";
        }

        CrewMember crew = rareOnly
            ? run.crewDatabase.GetRandomRareCrew(run.activeCrew)
            : run.crewDatabase.GetRandomCrew(run.activeCrew);

        if (crew == null)
        {
            string category = rareOnly ? "rare " : string.Empty;
            Debug.Log($"No unowned {category}crew members are available to recruit.", this);
            return $"No unowned {category}crew members were available to recruit.";
        }

        CrewAcquisitionResult result = run.TryRecruitCrew(crew);
        return run.GetCrewAcquisitionMessage(result, crew);
    }

    EffectResult ApplyRandomSimpleEffect()
    {
        RunManager run = RunManager.Instance;

        int roll = UnityEngine.Random.Range(0, 7);

        switch (roll)
        {
            case 0:
                return ApplyCreditChange(20);
            case 1:
                return ApplyCreditChange(-20);
            case 2:
                return ApplyFuelChange(10);
            case 3:
                return ApplyFuelChange(-10);
            case 4:
                return ApplyHealthChange(() => run.AddHealth(20));
            case 5:
                return ApplyHealthChange(() => run.DamageShip(15));
            case 6:
                return ApplyScrapGain(10);
        }

        return EffectResult.Continue("Nothing happened.");
    }

    static bool RollChance(int percent)
    {
        return UnityEngine.Random.Range(1, 101) <= percent;
    }

    EffectResult ApplyCreditChange(int amount)
    {
        RunManager run = RunManager.Instance;
        int before = run.money;

        if (amount > 0)
            run.AddMoney(amount);
        else if (amount < 0)
            run.LoseMoney(Mathf.Abs(amount));

        return EffectResult.Continue(DescribeResourceChange("Credits", before, run.money));
    }

    EffectResult ApplyFuelChange(int amount)
    {
        RunManager run = RunManager.Instance;
        int before = run.fuel;

        if (amount > 0)
            run.AddFuel(amount);
        else if (amount < 0)
            run.RemoveFuel(Mathf.Abs(amount));

        return EffectResult.Continue(DescribeResourceChange("Fuel", before, run.fuel));
    }

    EffectResult ApplyScrapGain(int amount)
    {
        RunManager run = RunManager.Instance;
        int before = run.scrap;

        if (amount > 0)
            run.AddScrap(amount);

        return EffectResult.Continue(DescribeResourceChange("Scrap", before, run.scrap));
    }

    EffectResult ApplyHealthChange(System.Action applyChange)
    {
        RunManager run = RunManager.Instance;
        int before = run.currentShipHealth;
        applyChange();
        int delta = run.currentShipHealth - before;

        if (delta > 0)
            return EffectResult.Continue($"Restored {delta} hull.");
        if (delta < 0)
            return EffectResult.Continue($"Took {-delta} hull damage.");
        return EffectResult.Continue("Hull integrity did not change.");
    }

    static string DescribeResourceChange(string resourceName, int before, int after)
    {
        int delta = after - before;
        if (delta > 0)
            return $"Gained {delta} {resourceName}.";
        if (delta < 0)
            return $"Lost {-delta} {resourceName}.";
        return $"{resourceName} did not change.";
    }

    static string BuildResolvedSummary(string flavourText, List<string> consequences)
    {
        string trimmedFlavour = string.IsNullOrWhiteSpace(flavourText)
            ? string.Empty
            : flavourText.Trim();
        string consequenceText = consequences == null || consequences.Count == 0
            ? string.Empty
            : string.Join(" ", consequences);

        if (string.IsNullOrEmpty(trimmedFlavour))
            return consequenceText;
        if (string.IsNullOrEmpty(consequenceText))
            return trimmedFlavour;

        return $"{trimmedFlavour}\n\n{consequenceText}";
    }

    readonly struct EffectResult
    {
        public readonly string summary;
        public readonly bool shouldContinue;

        EffectResult(string summary, bool shouldContinue)
        {
            this.summary = summary;
            this.shouldContinue = shouldContinue;
        }

        public static EffectResult Continue(string summary)
        {
            return new EffectResult(summary, true);
        }

        public static EffectResult Stop(string summary)
        {
            return new EffectResult(summary, false);
        }
    }
}
