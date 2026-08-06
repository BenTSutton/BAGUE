using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Surgeon")]
public class SurgeonEffect : CrewEffect
{
    [SerializeField, Min(0)] private int healingAfterCombat = 8;

    public override int GetPostCombatHealing()
    {
        return healingAfterCombat;
    }
}
