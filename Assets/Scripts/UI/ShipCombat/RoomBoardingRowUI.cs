using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RoomBoardingRowUI : MonoBehaviour
{
    private enum RowStatus
    {
        Uninitialized,
        Safe,
        Threatened,
        Damaged,
        Critical,
        Disabled
    }

    [Header("Room")]
    [SerializeField] private RoomHealth roomHealth;
    [SerializeField] private string roomNameOverride;

    [Header("Text")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text boarderCountText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text statusText;

    [Header("Images")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image rowBackgroundImage;
    [SerializeField] private Image damageFlashImage;

    [Header("Health Colours")]
    [SerializeField] private Color healthyColour = new Color(0.2f, 0.85f, 0.3f, 1f);
    [SerializeField] private Color strainedColour = new Color(0.95f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color damagedColour = new Color(1f, 0.45f, 0.1f, 1f);
    [SerializeField] private Color criticalColour = new Color(0.9f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color destroyedColour = new Color(0.2f, 0.2f, 0.2f, 1f);

    [Header("Row Colours")]
    [SerializeField] private Color safeRowColour = new Color(0.1f, 0.12f, 0.14f, 0.85f);
    [SerializeField] private Color threatenedRowColour = new Color(0.55f, 0.32f, 0.05f, 0.9f);
    [SerializeField] private Color criticalRowColour = new Color(0.55f, 0.05f, 0.05f, 0.95f);
    [SerializeField] private Color disabledRowColour = new Color(0.08f, 0.08f, 0.08f, 0.75f);

    [Header("Damage Flash")]
    [SerializeField] private Color damageFlashColour = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField, Min(0f)] private float damageFlashDuration = 0.18f;

    private int boarderCount = -1;
    private int displayedHealth;
    private int displayedMaximumHealth = 1;
    private bool healthInitialized;
    private RowStatus displayedStatus = RowStatus.Uninitialized;
    private Coroutine damageFlashRoutine;

    public RoomHealth RoomHealth => roomHealth;
    public int BoarderCount => Mathf.Max(0, boarderCount);

    private void Awake()
    {
        ResolveReferences();
        SetRoomName();
        SetDamageFlashAlpha(0f);
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (roomHealth == null)
        {
            Debug.LogWarning(
                "[RoomBoardingRowUI] RoomHealth reference is missing.",
                this);
            SetUnavailableState();
            return;
        }

        roomHealth.HealthChanged += HandleHealthChanged;
        roomHealth.Destroyed += HandleRoomDestroyed;

        RefreshHealth(roomHealth.currentHealth, roomHealth.MaxHealth);
        SetBoarderCount(Mathf.Max(0, boarderCount));
        healthInitialized = true;
    }

    private void Start()
    {
        // Corrects the initial display if this UI enabled before RoomHealth.Awake.
        if (roomHealth != null)
        {
            RefreshHealth(roomHealth.currentHealth, roomHealth.MaxHealth);
            healthInitialized = true;
        }
    }

    private void OnDisable()
    {
        if (roomHealth != null)
        {
            roomHealth.HealthChanged -= HandleHealthChanged;
            roomHealth.Destroyed -= HandleRoomDestroyed;
        }

        if (damageFlashRoutine != null)
        {
            StopCoroutine(damageFlashRoutine);
            damageFlashRoutine = null;
        }

        SetDamageFlashAlpha(0f);
        healthInitialized = false;
    }

    public void SetBoarderCount(int count)
    {
        count = Mathf.Max(0, count);

        if (boarderCount == count)
            return;

        boarderCount = count;

        if (boarderCountText != null)
        {
            boarderCountText.text = $"×{boarderCount}";
        }

        RefreshStatus();
    }

    private void HandleHealthChanged(int currentHealth, int maximumHealth)
    {
        bool tookDamage =
            healthInitialized && currentHealth < displayedHealth;

        RefreshHealth(currentHealth, maximumHealth);

        if (tookDamage)
        {
            PlayDamageFlash();
        }
    }

    private void HandleRoomDestroyed()
    {
        RefreshHealth(0, displayedMaximumHealth);
    }

    private void RefreshHealth(int currentHealth, int maximumHealth)
    {
        displayedMaximumHealth = Mathf.Max(1, maximumHealth);
        displayedHealth = Mathf.Clamp(
            currentHealth,
            0,
            displayedMaximumHealth);

        float healthPercentage =
            (float)displayedHealth / displayedMaximumHealth;

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = healthPercentage;
            healthFillImage.color = GetHealthColour(healthPercentage);
        }

        if (healthText != null)
        {
            healthText.text =
                $"{displayedHealth} / {displayedMaximumHealth}";
        }

        SetRoomName();
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        float healthPercentage = displayedMaximumHealth > 0
            ? (float)displayedHealth / displayedMaximumHealth
            : 0f;

        RowStatus newStatus;

        if (displayedHealth <= 0)
        {
            newStatus = RowStatus.Disabled;
        }
        else if (healthPercentage < 0.25f)
        {
            newStatus = RowStatus.Critical;
        }
        else if (boarderCount > 0)
        {
            newStatus = RowStatus.Threatened;
        }
        else if (healthPercentage < 0.5f)
        {
            newStatus = RowStatus.Damaged;
        }
        else
        {
            newStatus = RowStatus.Safe;
        }

        if (displayedStatus == newStatus)
            return;

        displayedStatus = newStatus;

        if (statusText != null)
        {
            statusText.text = GetStatusText(newStatus);
        }

        if (rowBackgroundImage != null)
        {
            rowBackgroundImage.color = GetRowColour(newStatus);
        }
    }

    private Color GetHealthColour(float healthPercentage)
    {
        if (displayedHealth <= 0)
            return destroyedColour;

        if (healthPercentage >= 0.75f)
            return healthyColour;

        if (healthPercentage >= 0.5f)
            return strainedColour;

        if (healthPercentage >= 0.25f)
            return damagedColour;

        return criticalColour;
    }

    private Color GetRowColour(RowStatus status)
    {
        return status switch
        {
            RowStatus.Threatened => threatenedRowColour,
            RowStatus.Critical => criticalRowColour,
            RowStatus.Disabled => disabledRowColour,
            _ => safeRowColour
        };
    }

    private static string GetStatusText(RowStatus status)
    {
        return status switch
        {
            RowStatus.Threatened => "THREATENED",
            RowStatus.Damaged => "DAMAGED",
            RowStatus.Critical => "CRITICAL",
            RowStatus.Disabled => "DISABLED",
            _ => "SAFE"
        };
    }

    private void SetRoomName()
    {
        if (roomNameText == null || roomHealth == null)
            return;

        if (!string.IsNullOrWhiteSpace(roomNameOverride))
        {
            roomNameText.text = roomNameOverride;
            return;
        }

        Room roomData = roomHealth.RoomData;
        roomNameText.text = roomData != null &&
            !string.IsNullOrWhiteSpace(roomData.roomName)
                ? roomData.roomName
                : roomHealth.gameObject.name.Replace('_', ' ');
    }

    private void PlayDamageFlash()
    {
        if (damageFlashImage == null || damageFlashDuration <= 0f)
            return;

        if (damageFlashRoutine != null)
        {
            StopCoroutine(damageFlashRoutine);
        }

        damageFlashRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        float elapsed = 0f;

        while (elapsed < damageFlashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / damageFlashDuration);
            SetDamageFlashAlpha(
                Mathf.Lerp(damageFlashColour.a, 0f, progress));
            yield return null;
        }

        SetDamageFlashAlpha(0f);
        damageFlashRoutine = null;
    }

    private void SetDamageFlashAlpha(float alpha)
    {
        if (damageFlashImage == null)
            return;

        Color colour = damageFlashColour;
        colour.a = alpha;
        damageFlashImage.color = colour;
    }

    private void SetUnavailableState()
    {
        if (boarderCountText != null)
            boarderCountText.text = "×--";

        if (healthText != null)
            healthText.text = "-- / --";

        if (statusText != null)
            statusText.text = "UNAVAILABLE";

        if (healthFillImage != null)
            healthFillImage.fillAmount = 0f;

        if (rowBackgroundImage != null)
            rowBackgroundImage.color = disabledRowColour;
    }

    private void ResolveReferences()
    {
        if (roomHealth == null)
        {
            roomHealth = GetComponentInParent<RoomHealth>();
        }
    }

    private void Reset()
    {
        ResolveReferences();
    }

    private void OnValidate()
    {
        ResolveReferences();
        damageFlashDuration = Mathf.Max(0f, damageFlashDuration);
    }
}
