using UnityEngine;

[CreateAssetMenu(menuName = "Crew Effects/Quartermaster")]
public class QuartermasterEffect : CrewEffect
{
    [SerializeField, Range(0f, 1f)] private float bonusFuelPercent = 0.5f;

    public override int ModifyFuelGain(int amount)
    {
        return Mathf.CeilToInt(amount * (1f + bonusFuelPercent));
    }
}
