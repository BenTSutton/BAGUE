using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RoomRepairButton : MonoBehaviour
{
    private static event Action AnyRoomRepairCompleted;

    [Header("Repair")]
    [SerializeField] private Room room;
    [SerializeField, Range(1f, 100f)] private float repairPercentage = 20f;
    [SerializeField, Min(0)] private int scrapCost = 99;

    [Header("UI")]
    [SerializeField] private Button repairButton;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private TMP_Text integrityText;

    private RoomInstance subscribedRoomInstance;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        AnyRoomRepairCompleted += Refresh;
        Refresh();
    }

    private void Start()
    {
        // OnEnable can run before RunManager.Awake when the Map scene is
        // launched directly. All Awake calls finish before Start runs.
        Refresh();
    }

    private void OnDisable()
    {
        AnyRoomRepairCompleted -= Refresh;
        UnsubscribeFromRoomIntegrity();
    }

    public void Repair()
    {
        if (!TryGetRoomInstance(out RoomInstance roomInstance))
        {
            Refresh();
            return;
        }

        int repairAmount = CalculateRepairAmount(
            roomInstance.MaximumIntegrity);

        bool repaired = RunManager.Instance.TryRepairRoom(
            room,
            repairAmount,
            scrapCost);

        if (repaired)
        {
            AnyRoomRepairCompleted?.Invoke();
        }
        else
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        ResolveReferences();

        if (!TryGetRoomInstance(out RoomInstance roomInstance))
        {
            UnsubscribeFromRoomIntegrity();
            SetUnavailableState();
            return;
        }

        SubscribeToRoomIntegrity(roomInstance);

        int currentIntegrity = roomInstance.CurrentIntegrity;
        int maximumIntegrity = roomInstance.MaximumIntegrity;
        bool fullyRepaired = currentIntegrity >= maximumIntegrity;
        bool canAffordRepair = RunManager.Instance.scrap >= scrapCost;

        if (repairButton != null)
        {
            repairButton.interactable =
                !fullyRepaired && canAffordRepair;
        }

        if (buttonText != null)
        {
            buttonText.text = fullyRepaired
                ? "FULLY REPAIRED"
                : $"Repair {repairPercentage:0.#}%: {scrapCost}";
        }

        if (integrityText != null)
        {
            integrityText.text =
                $"{currentIntegrity}%";
        }
    }

    private bool TryGetRoomInstance(out RoomInstance roomInstance)
    {
        roomInstance = null;

        if (RunManager.Instance == null || room == null)
            return false;

        roomInstance = RunManager.Instance.GetRoomInstance(room);

        return roomInstance != null && roomInstance.unlocked;
    }

    private int CalculateRepairAmount(int maximumIntegrity)
    {
        float normalizedPercentage = repairPercentage / 100f;

        return Mathf.Max(
            1,
            Mathf.CeilToInt(
                maximumIntegrity * normalizedPercentage));
    }

    private void SetUnavailableState()
    {
        if (repairButton != null)
        {
            repairButton.interactable = false;
        }

        if (buttonText != null)
        {
            buttonText.text = "REPAIR UNAVAILABLE";
        }

        if (integrityText != null)
        {
            integrityText.text = "-- / --";
        }
    }

    private void SubscribeToRoomIntegrity(RoomInstance roomInstance)
    {
        if (subscribedRoomInstance == roomInstance)
            return;

        UnsubscribeFromRoomIntegrity();

        subscribedRoomInstance = roomInstance;
        subscribedRoomInstance.IntegrityChanged += HandleIntegrityChanged;
    }

    private void UnsubscribeFromRoomIntegrity()
    {
        if (subscribedRoomInstance == null)
            return;

        subscribedRoomInstance.IntegrityChanged -= HandleIntegrityChanged;
        subscribedRoomInstance = null;
    }

    private void HandleIntegrityChanged(
        int currentIntegrity,
        int maximumIntegrity)
    {
        Refresh();
    }

    private void ResolveReferences()
    {
        if (repairButton == null)
        {
            repairButton = GetComponent<Button>();
        }

        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void Reset()
    {
        ResolveReferences();
    }

    private void OnValidate()
    {
        ResolveReferences();
        repairPercentage = Mathf.Clamp(repairPercentage, 1f, 100f);
        scrapCost = Mathf.Max(0, scrapCost);
    }
}
