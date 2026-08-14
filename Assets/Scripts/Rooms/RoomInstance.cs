using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class RoomInstance
{
    private const int DefaultMaximumIntegrity = 100;

    public Room roomData;
    public bool unlocked;
    public int level = 0;
    public List<CrewMember> assignedCrew = new List<CrewMember>();

    [Header("Persistent Integrity")]
    [SerializeField, Min(1)] private int maximumIntegrity;
    [SerializeField, Min(0)] private int currentIntegrity;
    [SerializeField, HideInInspector] private bool integrityInitialized;
    [SerializeField, HideInInspector] private bool wasDisabledThisCombat;

    public int MaximumIntegrity => integrityInitialized
        ? Mathf.Max(1, maximumIntegrity)
        : DefaultMaximumIntegrity;

    public int CurrentIntegrity => integrityInitialized
        ? Mathf.Clamp(currentIntegrity, 0, MaximumIntegrity)
        : MaximumIntegrity;

    public bool IsDestroyed => CurrentIntegrity <= 0;
    public bool IsIntegrityInitialized => integrityInitialized;
    public bool WasDisabledThisCombat => wasDisabledThisCombat;

    public event System.Action<int, int> IntegrityChanged;

    public bool CanUpgrade()
    {
        return unlocked && level < roomData.maxLevel;
    }

    public int GetUpgradeCost()
    {
        return roomData.GetUpgradeCost(level);
    }

    public void Upgrade()
    {
        if (!CanUpgrade()) return;

        level++;
        roomData.OnUpgrade(this);
    }

    public void InitializeIntegrityIfNeeded(
        int defaultMaximumIntegrity = DefaultMaximumIntegrity)
    {
        if (integrityInitialized && maximumIntegrity > 0)
        {
            int integrityBeforeClamp = currentIntegrity;
            currentIntegrity = Mathf.Clamp(
                currentIntegrity,
                0,
                maximumIntegrity);

            if (currentIntegrity != integrityBeforeClamp)
            {
                RaiseIntegrityChanged();
            }

            return;
        }

        maximumIntegrity = Mathf.Max(1, defaultMaximumIntegrity);
        currentIntegrity = maximumIntegrity;
        integrityInitialized = true;
        RaiseIntegrityChanged();
    }

    public void SetIntegrity(int current, int maximum)
    {
        int previousCurrentIntegrity = CurrentIntegrity;
        int previousMaximumIntegrity = MaximumIntegrity;
        bool wasInitialized = integrityInitialized;

        maximumIntegrity = Mathf.Max(1, maximum);
        currentIntegrity = Mathf.Clamp(current, 0, maximumIntegrity);
        integrityInitialized = true;

        if (!wasInitialized ||
            currentIntegrity != previousCurrentIntegrity ||
            maximumIntegrity != previousMaximumIntegrity)
        {
            RaiseIntegrityChanged();
        }
    }

    public int RepairIntegrity(int amount)
    {
        if (amount <= 0)
            return 0;

        InitializeIntegrityIfNeeded();

        int integrityBeforeRepair = currentIntegrity;
        currentIntegrity = Mathf.Min(
            currentIntegrity + amount,
            maximumIntegrity);

        int repairedIntegrity = currentIntegrity - integrityBeforeRepair;

        if (repairedIntegrity > 0)
        {
            RaiseIntegrityChanged();
        }

        return repairedIntegrity;
    }

    public int RecoverDestroyedIntegrity(float recoveryPercentage)
    {
        InitializeIntegrityIfNeeded();

        if (currentIntegrity > 0)
            return 0;

        int recoveredIntegrity = Mathf.CeilToInt(
            maximumIntegrity * Mathf.Clamp01(recoveryPercentage));

        currentIntegrity = Mathf.Clamp(
            recoveredIntegrity,
            1,
            maximumIntegrity);

        RaiseIntegrityChanged();
        return currentIntegrity;
    }

    public void ResetIntegrity()
    {
        if (!integrityInitialized)
            return;

        if (currentIntegrity == MaximumIntegrity)
            return;

        currentIntegrity = MaximumIntegrity;
        RaiseIntegrityChanged();
    }

    public void MarkDisabledThisCombat()
    {
        wasDisabledThisCombat = true;
    }

    public void ResetCombatState()
    {
        wasDisabledThisCombat = false;
    }

    private void RaiseIntegrityChanged()
    {
        IntegrityChanged?.Invoke(CurrentIntegrity, MaximumIntegrity);
    }
}
