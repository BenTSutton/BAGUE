using TMPro;
using UnityEngine;

public sealed class BoarderIntentIndicator : MonoBehaviour
{
    private const string PlayerTargetLabel = "TARGET: YOU";
    private const string NoTargetLabel = "NO TARGET";

    private enum DisplayedIntent
    {
        Uninitialized,
        NoTarget,
        Player,
        Room
    }

    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private TMP_Text intentLabel;

    [Header("Threat Colours")]
    [SerializeField] private Color playerThreatColour = new Color(1f, 0.2f, 0.15f, 1f);
    [SerializeField] private Color roomThreatColour = new Color(1f, 0.75f, 0.1f, 1f);
    [SerializeField] private Color noTargetColour = new Color(0.7f, 0.7f, 0.7f, 1f);

    [Header("Refresh")]
    [Tooltip("How often the indicator checks the AI target. Text is only changed when the intent changes.")]
    [SerializeField, Min(0.02f)] private float refreshInterval = 0.1f;

    private DisplayedIntent displayedIntent = DisplayedIntent.Uninitialized;
    private RoomHealth cachedRoom;
    private string cachedRoomLabel;
    private float nextRefreshTime;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        displayedIntent = DisplayedIntent.Uninitialized;
        nextRefreshTime = 0f;
    }

    private void Start()
    {
        if (enemyAI == null)
        {
            Debug.LogWarning(
                "[BoarderIntentIndicator] EnemyAI reference is missing.",
                this);
        }

        if (intentLabel == null)
        {
            Debug.LogWarning(
                "[BoarderIntentIndicator] TextMeshPro label reference is missing.",
                this);
            return;
        }

        RefreshIntent();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefreshTime)
            return;

        nextRefreshTime = Time.unscaledTime + refreshInterval;
        RefreshIntent();
    }

    private void RefreshIntent()
    {
        if (intentLabel == null)
            return;

        if (enemyAI == null || !enemyAI.isActiveAndEnabled)
        {
            ShowNoTarget();
            return;
        }

        Transform movementTarget = enemyAI.SelectMovementTarget();

        if (movementTarget != null && movementTarget == enemyAI.player)
        {
            ApplyIntent(
                DisplayedIntent.Player,
                PlayerTargetLabel,
                playerThreatColour);
            return;
        }

        RoomHealth assignedRoom = enemyAI.assignedRoom;

        if (assignedRoom != null && movementTarget == assignedRoom.transform)
        {
            CacheRoomLabel(assignedRoom);

            ApplyIntent(
                DisplayedIntent.Room,
                cachedRoomLabel,
                roomThreatColour);
            return;
        }

        ShowNoTarget();
    }

    private void ShowNoTarget()
    {
        ApplyIntent(
            DisplayedIntent.NoTarget,
            NoTargetLabel,
            noTargetColour);
    }

    private void CacheRoomLabel(RoomHealth room)
    {
        if (cachedRoom == room && !string.IsNullOrEmpty(cachedRoomLabel))
            return;

        cachedRoom = room;
        cachedRoomLabel = "TARGET: " + room.gameObject.name;
    }

    private void ApplyIntent(
        DisplayedIntent newIntent,
        string newText,
        Color newColour)
    {
        if (displayedIntent == newIntent &&
            intentLabel.text == newText &&
            intentLabel.color == newColour)
        {
            return;
        }

        displayedIntent = newIntent;
        intentLabel.text = newText;
        intentLabel.color = newColour;
    }

    private void ResolveReferences()
    {
        if (enemyAI == null)
        {
            enemyAI = GetComponentInParent<EnemyAI>();
        }

        if (intentLabel == null)
        {
            intentLabel = GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void Reset()
    {
        ResolveReferences();
    }

    private void OnValidate()
    {
        ResolveReferences();
        refreshInterval = Mathf.Max(0.02f, refreshInterval);
    }
}
