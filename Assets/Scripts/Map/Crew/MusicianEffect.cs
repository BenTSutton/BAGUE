using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Musician")]
public class MusicianEffect : CrewEffect
{
    [SerializeField, Min(0)] private int bonusHealing = 1;

    public override int ModifyHealing(int amount) => amount + bonusHealing;
}
