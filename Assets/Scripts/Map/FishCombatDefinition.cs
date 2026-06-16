using UnityEngine;

public enum CombatType
{
    Aggressive,
    Tank
}

[CreateAssetMenu(menuName = "Map/Node Content/Combat/Fish Combat")]
public class FishCombatDefinition : CombatDefinition
{
    [SerializeField]
    private CombatType _combatType;

    public override CombatType combatType => _combatType;
}