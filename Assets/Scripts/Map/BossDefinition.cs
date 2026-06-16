using UnityEngine;


[CreateAssetMenu(menuName = "Map/Node Content/Boss")]
public class BossDefinition : CombatDefinition
{
    [SerializeField]
    private CombatType _combatType;

    public override CombatType combatType => _combatType;
}