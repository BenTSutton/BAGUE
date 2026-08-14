using System;
using UnityEngine;

public class RoomHealth : MonoBehaviour
{
    [Header("Persistent Room")]
    [SerializeField] private Room roomData;

    [Header("Integrity")]
    [SerializeField] private int maxHealth = 100;

    public int currentHealth;
    public bool IsDestroyed => currentHealth <= 0;
    public int MaxHealth => maxHealth;
    public Room RoomData => roomData;

    public event Action<int, int> HealthChanged;
    public event Action Destroyed;
    public static event Action<RoomHealth> RoomDisabled;

    private const float DestroyedRoomRecoveryPercentage = 0.25f;

    private RoomInstance persistentRoomInstance;
    private bool combatResolved;
    private bool bindingWarningIssued;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
        TryBindPersistentRoom();
    }

    private void OnEnable()
    {
        GameManager.CombatResolutionStarted += HandleCombatResolutionStarted;
    }

    private void Start()
    {
        // This second attempt handles direct combat-scene testing where
        // RunManager may awaken after the room objects.
        if (persistentRoomInstance == null && TryBindPersistentRoom())
        {
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    private void OnDisable()
    {
        GameManager.CombatResolutionStarted -= HandleCombatResolutionStarted;
        PersistIntegrity();
    }

    public void TakeDamage(int damage)
    {
        if (combatResolved || IsDestroyed || damage <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        SFXManager.Instance?.PlayRoomTakeDamage(transform.position);
        PersistIntegrity();
        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (IsDestroyed)
        {
            persistentRoomInstance?.MarkDisabledThisCombat();
            Destroyed?.Invoke();
            RoomDisabled?.Invoke(this);
        }
    }

    private bool TryBindPersistentRoom()
    {
        if (roomData == null)
        {
            WarnAboutMissingBinding(
                "No Room asset is assigned. This room will use combat-local health.");
            return false;
        }

        if (RunManager.Instance == null)
            return false;

        RoomInstance roomInstance = RunManager.Instance.GetRoomInstance(roomData);

        if (roomInstance == null)
        {
            WarnAboutMissingBinding(
                $"No matching RoomInstance was found for {roomData.roomName}. " +
                "This room will use combat-local health.");
            return false;
        }

        roomInstance.InitializeIntegrityIfNeeded(maxHealth);

        persistentRoomInstance = roomInstance;
        maxHealth = roomInstance.MaximumIntegrity;
        currentHealth = roomInstance.CurrentIntegrity;
        return true;
    }

    private void PersistIntegrity()
    {
        if (persistentRoomInstance == null)
            return;

        persistentRoomInstance.SetIntegrity(currentHealth, maxHealth);
    }

    private void HandleCombatResolutionStarted()
    {
        combatResolved = true;

        if (!IsDestroyed)
            return;

        if (persistentRoomInstance != null)
        {
            persistentRoomInstance.RecoverDestroyedIntegrity(
                DestroyedRoomRecoveryPercentage);
            currentHealth = persistentRoomInstance.CurrentIntegrity;
        }
        else
        {
            currentHealth = Mathf.Clamp(
                Mathf.CeilToInt(
                    maxHealth * DestroyedRoomRecoveryPercentage),
                1,
                maxHealth);
        }

        PersistIntegrity();
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void WarnAboutMissingBinding(string message)
    {
        if (bindingWarningIssued)
            return;

        bindingWarningIssued = true;
        Debug.LogWarning($"[RoomHealth] {message}", this);
    }
}
