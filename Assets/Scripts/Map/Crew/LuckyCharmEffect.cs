using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Lucky Charm")]
public class LuckyCharmEffect : CrewEffect
{
    [SerializeField, Range(0f, 1f)] private float avoidDamageChance = 0.2f;

    public override int ModifyDamageTaken(int amount)
    {
        return Random.value < avoidDamageChance ? 0 : amount;
    }
}
