using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Crew/Crew Database")]
public class CrewDatabase : ScriptableObject
{
    public List<CrewMember> crewMembers;

    private static readonly CrewRecruitmentCategory[] NormalRecruitmentCategories =
    {
        CrewRecruitmentCategory.Common,
        CrewRecruitmentCategory.Uncommon
    };

    public CrewMember GetByName(string name)
    {
        if (crewMembers == null || string.IsNullOrWhiteSpace(name))
            return null;

        return crewMembers
            .FirstOrDefault(crew => crew != null && crew.crewName == name);
    }

    public CrewMember GetRandomCrew(IEnumerable<CrewMember> excludedCrew = null)
    {
        return GetRandomCrewFromCategories(NormalRecruitmentCategories, excludedCrew);
    }

    public CrewMember GetRandomRareCrew(IEnumerable<CrewMember> excludedCrew = null)
    {
        return GetRandomCrewFromCategories(
            new[] { CrewRecruitmentCategory.Rare },
            excludedCrew);
    }

    public CrewMember GetRandomCrewFromCategories(
        IEnumerable<CrewRecruitmentCategory> categories,
        IEnumerable<CrewMember> excludedCrew = null)
    {
        return PickRandom(GetEligibleCrew(categories, excludedCrew));
    }

    public bool HasRecruitableCrew(
        IEnumerable<CrewRecruitmentCategory> categories,
        IEnumerable<CrewMember> excludedCrew = null)
    {
        return GetEligibleCrew(categories, excludedCrew).Any();
    }

    public CrewMember GetRandomPurchasableCrew(IEnumerable<CrewMember> excludedCrew)
    {
        HashSet<CrewMember> excluded = BuildExclusionSet(excludedCrew);
        IEnumerable<CrewMember> eligibleCrew = AllConfiguredCrew()
            .Where(crew => crew.purchasable
                && crew.recruitmentCategory != CrewRecruitmentCategory.EventOnly
                && !excluded.Contains(crew));

        return PickRandom(eligibleCrew);
    }

    private IEnumerable<CrewMember> GetEligibleCrew(
        IEnumerable<CrewRecruitmentCategory> categories,
        IEnumerable<CrewMember> excludedCrew)
    {
        HashSet<CrewRecruitmentCategory> allowedCategories = categories != null
            ? new HashSet<CrewRecruitmentCategory>(categories)
            : new HashSet<CrewRecruitmentCategory>();
        HashSet<CrewMember> excluded = BuildExclusionSet(excludedCrew);

        return AllConfiguredCrew().Where(crew =>
            allowedCategories.Contains(crew.recruitmentCategory)
            && !excluded.Contains(crew));
    }

    private IEnumerable<CrewMember> AllConfiguredCrew()
    {
        return crewMembers?.Where(crew => crew != null)
            ?? Enumerable.Empty<CrewMember>();
    }

    private static HashSet<CrewMember> BuildExclusionSet(
        IEnumerable<CrewMember> excludedCrew)
    {
        return excludedCrew != null
            ? new HashSet<CrewMember>(excludedCrew.Where(crew => crew != null))
            : new HashSet<CrewMember>();
    }

    private static CrewMember PickRandom(IEnumerable<CrewMember> crew)
    {
        List<CrewMember> pool = crew.ToList();
        return pool.Count == 0
            ? null
            : pool[Random.Range(0, pool.Count)];
    }
}
