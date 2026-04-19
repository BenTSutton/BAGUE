using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Rooms")]
public class Room : ScriptableObject
{

    public string roomDescription;
    public bool unlocked;
    public Sprite roomLogoSprite;

    public List<CrewMember> assignedCrew;

    public virtual void OnEnter() {}

}
