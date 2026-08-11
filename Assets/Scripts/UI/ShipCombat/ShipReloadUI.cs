using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CannonReloadUI : MonoBehaviour
{
    [SerializeField] private Cannon cannon;
    [SerializeField] private Slider reloadSlider;
    [SerializeField] private GameObject readyIndicator;

    [Header("Weapons Room Disabled")]
    [SerializeField] private GameObject reloadPausedIndicator;
    [SerializeField] private TMP_Text reloadPausedText;

    [Header("Audio")]
    [SerializeField] private AudioClip readySound;


    private void OnEnable()
    {
        if (cannon == null)
        {
            return;
        }

        cannon.ReloadProgressChanged += HandleReloadProgress;
        cannon.CannonReady += HandleCannonReady;
        cannon.ReloadPauseChanged += HandleReloadPauseChanged;
    }

    private void Start()
    {
        if (cannon == null)
        {
            Debug.LogWarning("[CannonReloadUI] Cannon reference is missing.", this);

            if (reloadPausedIndicator != null)
            {
                reloadPausedIndicator.SetActive(false);
            }

            return;
        }

        HandleReloadProgress(cannon.ReloadProgress);
        HandleReloadPauseChanged(cannon.IsReloadPaused);
    }

    private void OnDisable()
    {
        if (cannon == null)
        {
            return;
        }

        cannon.ReloadProgressChanged -= HandleReloadProgress;
        cannon.CannonReady -= HandleCannonReady;
        cannon.ReloadPauseChanged -= HandleReloadPauseChanged;
    }

    private void HandleReloadProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        if (reloadSlider != null)
        {
            reloadSlider.value = progress;
        }

        if (readyIndicator != null)
        {
            readyIndicator.SetActive(progress >= 1f);
        }

        RefreshPausedMessage();
    }

    private void HandleCannonReady()
    {
        HandleReloadProgress(1f);

        if (readySound != null && SFXManager.Instance != null)
        {
            SFXManager.Instance.PlaySFX(readySound);
        }
    }

    private void HandleReloadPauseChanged(bool paused)
    {
        if (reloadPausedIndicator != null)
        {
            reloadPausedIndicator.SetActive(paused);
        }

        RefreshPausedMessage();
    }

    private void RefreshPausedMessage()
    {
        if (cannon == null ||
            reloadPausedText == null ||
            !cannon.IsReloadPaused)
        {
            return;
        }

        reloadPausedText.text = cannon.IsLoaded
            ? "LOADED SHOT AVAILABLE"
            : "RELOAD PAUSED";
    }
}