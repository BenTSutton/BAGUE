using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RoomDisplay : MonoBehaviour
{
    [Header("Room")]
    [SerializeField] private RoomHealth roomHealth;
    [SerializeField] private string roomNameOverride;

    [Tooltip("Used before the first HealthChanged event. Match this to RoomHealth's Max Health value.")]
    [SerializeField, Min(1)] private int initialMaximumHealth = 100;

    [Header("UI")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text healthText;

    [Header("Condition Colours")]
    [SerializeField] private Color healthyColour = new Color(0.2f, 0.85f, 0.3f, 1f);
    [SerializeField] private Color strainedColour = new Color(0.95f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color damagedColour = new Color(1f, 0.45f, 0.1f, 1f);
    [SerializeField] private Color criticalColour = new Color(0.9f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color destroyedColour = new Color(0.2f, 0.2f, 0.2f, 1f);

    [Header("Damage Flash")]
    [Tooltip("Optional overlay Image. If omitted, the health fill itself flashes.")]
    [SerializeField] private Image damageFlashImage;
    [SerializeField] private Color damageFlashColour = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField, Min(0f)] private float damageFlashDuration = 0.18f;

    private int displayedHealth;
    private int lastKnownMaximumHealth;
    private bool displayInitialized;
    private Coroutine damageFlashRoutine;

    private void Awake()
    {
        ResolveRoomHealth();

        lastKnownMaximumHealth = Mathf.Max(initialMaximumHealth, 1);

        if (roomHealth != null)
        {
            // RoomHealth currently begins each combat at full health. Capturing its
            // initial value avoids requiring a change to RoomHealth's public API.
            lastKnownMaximumHealth = Mathf.Max(
                lastKnownMaximumHealth,
                roomHealth.currentHealth);
        }
    }

    private void OnEnable()
    {
        ResolveRoomHealth();

        if (roomHealth == null)
        {
            Debug.LogWarning(
                "[RoomDisplay] No RoomHealth reference was assigned or found in a parent.",
                this);
            return;
        }

        roomHealth.HealthChanged += HandleHealthChanged;
        roomHealth.Destroyed += HandleDestroyed;

        RefreshDisplay(roomHealth.currentHealth, lastKnownMaximumHealth);
        displayInitialized = true;
    }

    private void OnDisable()
    {
        if (roomHealth != null)
        {
            roomHealth.HealthChanged -= HandleHealthChanged;
            roomHealth.Destroyed -= HandleDestroyed;
        }

        if (damageFlashRoutine != null)
        {
            StopCoroutine(damageFlashRoutine);
            damageFlashRoutine = null;
        }

        RestoreFlashVisual();
        displayInitialized = false;
    }

    void Start()
    {
        RefreshDisplay(roomHealth.currentHealth, lastKnownMaximumHealth);
    }

    private void HandleHealthChanged(int currentHealth, int maximumHealth)
    {
        int previousHealth = displayedHealth;
        lastKnownMaximumHealth = Mathf.Max(maximumHealth, 1);

        RefreshDisplay(currentHealth, lastKnownMaximumHealth);

        if (displayInitialized && currentHealth < previousHealth)
        {
            PlayDamageFlash();
        }
    }

    private void HandleDestroyed()
    {
        RefreshDisplay(0, lastKnownMaximumHealth);
    }

    private void RefreshDisplay(int currentHealth, int maximumHealth)
    {
        maximumHealth = Mathf.Max(maximumHealth, 1);
        currentHealth = Mathf.Clamp(currentHealth, 0, maximumHealth);

        displayedHealth = currentHealth;
        lastKnownMaximumHealth = maximumHealth;

        float healthPercentage = (float)currentHealth / maximumHealth;

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = healthPercentage;
            healthFillImage.color = GetConditionColour(
                currentHealth,
                healthPercentage);
        }

        if (roomNameText != null)
        {
            roomNameText.text = string.IsNullOrWhiteSpace(roomNameOverride)
                ? roomHealth.gameObject.name.Replace('_', ' ')
                : roomNameOverride;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maximumHealth}";
        }
    }

    private Color GetConditionColour(int currentHealth, float healthPercentage)
    {
        if (currentHealth <= 0)
            return destroyedColour;

        if (healthPercentage >= 0.75f)
            return healthyColour;

        if (healthPercentage >= 0.5f)
            return strainedColour;

        if (healthPercentage >= 0.25f)
            return damagedColour;

        return criticalColour;
    }

    private void PlayDamageFlash()
    {
        if (damageFlashRoutine != null)
        {
            StopCoroutine(damageFlashRoutine);
        }

        damageFlashRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        Image flashTarget = damageFlashImage != null
            ? damageFlashImage
            : healthFillImage;

        if (flashTarget == null || damageFlashDuration <= 0f)
        {
            damageFlashRoutine = null;
            yield break;
        }

        bool usingOverlay = flashTarget != healthFillImage;
        Color restingColour = GetConditionColour(
            displayedHealth,
            (float)displayedHealth / lastKnownMaximumHealth);
        Color transparentFlashColour = damageFlashColour;
        transparentFlashColour.a = 0f;

        float elapsed = 0f;

        while (elapsed < damageFlashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / damageFlashDuration);

            flashTarget.color = usingOverlay
                ? Color.Lerp(damageFlashColour, transparentFlashColour, progress)
                : Color.Lerp(damageFlashColour, restingColour, progress);

            yield return null;
        }

        RestoreFlashVisual();
        damageFlashRoutine = null;
    }

    private void RestoreFlashVisual()
    {
        if (damageFlashImage != null && damageFlashImage != healthFillImage)
        {
            Color transparentColour = damageFlashColour;
            transparentColour.a = 0f;
            damageFlashImage.color = transparentColour;
        }

        if (healthFillImage != null && lastKnownMaximumHealth > 0)
        {
            healthFillImage.color = GetConditionColour(
                displayedHealth,
                (float)displayedHealth / lastKnownMaximumHealth);
        }
    }

    private void ResolveRoomHealth()
    {
        if (roomHealth == null)
        {
            roomHealth = GetComponentInParent<RoomHealth>();
        }
    }

    private void Reset()
    {
        ResolveRoomHealth();
    }

    private void OnValidate()
    {
        ResolveRoomHealth();
        initialMaximumHealth = Mathf.Max(initialMaximumHealth, 1);
        damageFlashDuration = Mathf.Max(damageFlashDuration, 0f);
    }
}
