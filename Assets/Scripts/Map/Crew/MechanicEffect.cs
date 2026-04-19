using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Mechanic")]
public class MechanicEffect : CrewEffect
{
    public override int ModifyScrapGain(int amount) => amount + 1;
}