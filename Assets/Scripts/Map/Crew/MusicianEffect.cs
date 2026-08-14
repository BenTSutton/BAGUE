using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Musician")]
public class MusicianEffect : CrewEffect
{
    [SerializeField, Range(0f, 1f)] private float bonusHealingPercent = 0.3f;

    public override int ModifyHealing(int amount)
    {
        return Mathf.CeilToInt(amount * (1f + bonusHealingPercent));
    }
}
