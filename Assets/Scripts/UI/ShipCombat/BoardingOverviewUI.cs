using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class BoardingOverviewUI : MonoBehaviour
{
    [Header("Rows")]
    [SerializeField] private List<RoomBoardingRowUI> roomRows = new();

    [Header("Summary")]
    [SerializeField] private TMP_Text totalBoardersText;
    [SerializeField] private string totalBoardersPrefix = "BOARDERS ABOARD: ";

    [Header("Refresh")]
    [SerializeField, Min(0.05f)] private float refreshInterval = 0.2f;

    private readonly Dictionary<RoomHealth, int> countsByRoom = new();

    private float nextRefreshTime;
    private int displayedTotal = -1;

    private void Awake()
    {
        ResolveRows();
    }

    private void OnEnable()
    {
        displayedTotal = -1;
        nextRefreshTime = 0f;
        RefreshNow();
    }

    private void Start()
    {
        // Ensures scene objects and persistent RoomHealth bindings are ready.
        RefreshNow();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefreshTime)
            return;

        nextRefreshTime = Time.unscaledTime + refreshInterval;
        RefreshNow();
    }

    public void RefreshNow()
    {
        ResolveRows();
        countsByRoom.Clear();

        EnemyAI[] activeBoarders =
            Object.FindObjectsByType<EnemyAI>(
                FindObjectsSortMode.None);

        int totalBoarders = 0;

        foreach (EnemyAI boarder in activeBoarders)
        {
            if (boarder == null ||
                !boarder.isActiveAndEnabled ||
                !boarder.gameObject.activeInHierarchy)
            {
                continue;
            }

            totalBoarders++;

            RoomHealth assignedRoom = boarder.assignedRoom;

            if (assignedRoom == null)
                continue;

            countsByRoom.TryGetValue(
                assignedRoom,
                out int currentCount);

            countsByRoom[assignedRoom] = currentCount + 1;
        }

        foreach (RoomBoardingRowUI roomRow in roomRows)
        {
            if (roomRow == null)
                continue;

            RoomHealth room = roomRow.RoomHealth;
            int count = 0;

            if (room != null)
            {
                countsByRoom.TryGetValue(room, out count);
            }

            roomRow.SetBoarderCount(count);
        }

        RefreshSummary(totalBoarders);
    }

    private void RefreshSummary(int totalBoarders)
    {
        if (displayedTotal == totalBoarders)
            return;

        displayedTotal = totalBoarders;

        if (totalBoarders != 0)
        {
            totalBoardersText.text = totalBoardersPrefix + totalBoarders;
        }
        else
        {
            totalBoardersText.text = "Ship Secure";
        }
    }

    private void ResolveRows()
    {
        roomRows.RemoveAll(row => row == null);

        if (roomRows.Count > 0)
            return;

        roomRows.AddRange(
            GetComponentsInChildren<RoomBoardingRowUI>(true));
    }

    private void OnValidate()
    {
        refreshInterval = Mathf.Max(0.05f, refreshInterval);
    }
}
