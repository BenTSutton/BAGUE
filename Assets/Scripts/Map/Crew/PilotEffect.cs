using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Pilot")]
public class PilotEffect : CrewEffect
{
    [SerializeField, Range(0f, 100f)] private float dodgeChanceBonus = 10f;

    public override float ModifyDodgeChance(float chance) => chance + dodgeChanceBonus;
}
