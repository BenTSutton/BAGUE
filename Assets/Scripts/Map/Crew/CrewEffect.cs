using UnityEngine;

public abstract class CrewEffect : ScriptableObject
{
    public virtual int ModifyScrapGain(int amount) => amount;
    public virtual int ModifyFuelGain(int amount) => amount;
    public virtual int ModifyMoneyGain(int amount) => amount;
    public virtual int ModifyHealing(int amount) => amount;
    public virtual int ModifyDamageTaken(int amount) => amount;
    public virtual float ModifyCannonDamage(float amount, int shotNumber) => amount;
    public virtual float ModifyDodgeChance(float chance) => chance;
    public virtual int ModifyJumpFuelCost(int cost) => cost;
}
