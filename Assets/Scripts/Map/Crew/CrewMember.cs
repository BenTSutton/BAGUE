using UnityEngine;

public enum CrewRecruitmentCategory
{
    Common,
    Uncommon,
    Rare,
    EventOnly
}

[CreateAssetMenu(menuName = "Crew/Crew Member")]
public class CrewMember : ScriptableObject
{
    public string crewName;
    public Sprite icon;
    public string powerName;
    [TextArea] public string description;
    public CrewEffect crewEffect;
    public CrewRecruitmentCategory recruitmentCategory = CrewRecruitmentCategory.Common;
    public bool purchasable;
    public int price;
}
