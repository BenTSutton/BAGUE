using UnityEngine;

[CreateAssetMenu(menuName = "Crew/Crew Member")]
public class CrewMember : ScriptableObject
{
    public string crewName;
    public Sprite icon;
    [TextArea] public string description;
    public CrewEffect crewEffect;
    public bool purchasable;
    public int price;
}