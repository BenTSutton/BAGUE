using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDefeatEffects : MonoBehaviour
{
    [Header("Ship")]
    [SerializeField] private Image shipImage;
    [SerializeField] private Color damagedTint = new Color(0.35f, 0.08f, 0.04f, 1f);
    [SerializeField, Min(0f)] private float flickerDuration = 2.5f;
    [SerializeField, Min(1f)] private float flickerSpeed = 14f;
    [SerializeField] private CanvasGroup shipVisuals;

    [Header("Explosion Flashes")]
    [SerializeField] private CanvasGroup[] explosionFlashes;
    [SerializeField, Min(0.05f)] private float flashDuration = 0.35f;
    [SerializeField, Min(0.1f)] private float flashScale = 1.5f;
    [SerializeField, Min(0f)] private float delayBetweenFlashes = 0.12f;

    [Header("Optional Audio")]
    [SerializeField] private AudioClip shutdownSound;
    [SerializeField] private AudioClip stagedBlastSound;
    [SerializeField] private AudioClip finalBlastSound;

    private Color originalShipColour = Color.white;
    private Coroutine flickerRoutine;

    private void Awake()
    {
        if(shipImage != null) 
        {
            originalShipColour = shipImage.color;
        }

        if(explosionFlashes == null) 
        {
            return;
        }

        foreach(CanvasGroup flash in explosionFlashes)
        {
            if(flash == null) continue;
            flash.alpha = 0f;
            flash.gameObject.SetActive(false);
        }

        if (shipVisuals != null)
        {
            shipVisuals.alpha = 1f;
        }
    }

    private void OnEnable()
    {
        EnemyShip.OnEnemyShipDefeated += HandleDefeatStarted;
        EnemyShip.OnEnemyShipStagedExplosion += HandleStagedExplosion;
        EnemyShip.OnEnemyShipFinalExplosion += HandleFinalExplosion;
    }

    private void OnDisable()
    {
        EnemyShip.OnEnemyShipDefeated -= HandleDefeatStarted;
        EnemyShip.OnEnemyShipStagedExplosion -= HandleStagedExplosion;
        EnemyShip.OnEnemyShipFinalExplosion -= HandleFinalExplosion;
    }

    private void HandleDefeatStarted(EnemyShip defeatedShip)
    {
        if (defeatedShip == null)
        {
            return;
        }

        BeginShutdown();
    }

    private void HandleStagedExplosion(EnemyShip defeatedShip)
    {
        if (defeatedShip == null)
        {
            return;
        }

        PlayStagedExplosion();
    }

    private void HandleFinalExplosion(EnemyShip defeatedShip)
    {
        if (defeatedShip == null)
        {
            return;
        }

        PlayFinalExplosion();
    }

    public void BeginShutdown()
    {
        MusicManager.Instance?.PlayVictoryMusic();
        SFXManager.Instance?.PlaySFX(shutdownSound);

        if(shipImage == null) 
        {
            return;
        }

        if(flickerRoutine != null) 
        {
            StopCoroutine(flickerRoutine);
        }

        flickerRoutine = StartCoroutine(FlickerShip());
    }

    public void PlayStagedExplosion()
        {
        SFXManager.Instance?.PlaySFX(stagedBlastSound);

        if(CameraShake.Instance != null) 
        {
            CameraShake.Instance.TriggerShake(0.08f, 5f);
        }
        
        StartCoroutine(PlayFlashesInSequence());
    }

    public void PlayFinalExplosion()
    {
        if(flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }

        SFXManager.Instance?.PlaySFX(finalBlastSound);

        if(CameraShake.Instance != null) 
        {
            CameraShake.Instance.TriggerShake(0.25f, 12f);
        }

        if(explosionFlashes != null)
        {
            foreach(CanvasGroup flash in explosionFlashes)
            {
                if(flash != null) 
                {
                    StartCoroutine(Flash(flash, flashDuration, flashScale * 1.5f));
                }
            }
        }

        if(shipImage != null) 
        {
            StartCoroutine(FadeHull());
        }
    }

    private IEnumerator FlickerShip()
    {
        float elapsed = 0f;

        while(elapsed < flickerDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float flicker = Mathf.PingPong(elapsed * flickerSpeed, 1f);
            shipImage.color = Color.Lerp(originalShipColour, damagedTint, flicker * 0.8f);
            yield return null;
        }

        shipImage.color = Color.Lerp(originalShipColour, damagedTint, 0.6f);
        flickerRoutine = null;
    }

    private IEnumerator PlayFlashesInSequence()
    {
        if(explosionFlashes == null) 
        {
            yield break;
        }

        foreach(CanvasGroup flash in explosionFlashes)
        {
            if(flash == null) 
            {
                continue;
            }

            StartCoroutine(Flash(flash, flashDuration, flashScale));

            if(delayBetweenFlashes > 0f) 
            {
                yield return new WaitForSecondsRealtime(delayBetweenFlashes);
            }
        }
    }

    private IEnumerator Flash(CanvasGroup flash, float duration, float maximumScale)
    {
        flash.gameObject.SetActive(true);

        RectTransform rect = flash.transform as RectTransform;
        Vector3 originalScale = rect != null ? rect.localScale : Vector3.one;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float brightness = Mathf.Sin(progress * Mathf.PI);

            flash.alpha = brightness;

            if(rect != null)
            {
                float scale = Mathf.Lerp(0.25f, maximumScale, progress);
                rect.localScale = originalScale * scale;
            }

            yield return null;
        }

        flash.alpha = 0f;

        if(rect != null) 
        {
            rect.localScale = originalScale;
        }
        
        flash.gameObject.SetActive(false);
    }

    private IEnumerator FadeHull()
    {
        float duration = 0.3f;
        float elapsed = 0f;

        Color startingColour = Color.white;
        Color endingColour = new Color(0.05f, 0.05f, 0.05f, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            if (shipImage != null)
            {
                shipImage.color = Color.Lerp(startingColour, endingColour, progress);
            }

            if (shipVisuals != null)
            {
                shipVisuals.alpha = 1f - progress;
            }

            yield return null;
        }

        if (shipVisuals != null)
        {
            shipVisuals.alpha = 0f;
        }
    }
}