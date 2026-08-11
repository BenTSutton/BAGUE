using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum EnemyHitType
{
    Environment,
    Melee,
    Gun
}

public class CombatFeedback : MonoBehaviour
{
    public static CombatFeedback Instance { get; private set; }

    [Header("Sounds")]
    [SerializeField] private AudioClip meleeSwing;
    [SerializeField] private AudioClip meleeImpact;
    [SerializeField] private AudioClip gunshot;
    [SerializeField] private AudioClip gunImpact;
    [SerializeField] private AudioClip parry;
    [SerializeField] private AudioClip enemyKilled;
    [SerializeField] private AudioClip playerHurt;

    [Header("Hit-stop")]
    [SerializeField, Range(0f, 1f)] private float hitStopScale = 0.05f;
    [SerializeField] private float meleeHitStop = 0.045f;
    [SerializeField] private float gunHitStop = 0.065f;
    [SerializeField] private float killHitStop = 0.08f;
    [SerializeField] private float parryHitStop = 0.07f;
    [SerializeField] private float playerDamageHitStop = 0.055f;

    [Header("Camera shake")]
    [SerializeField] private float meleeShake = 0.5f;
    [SerializeField] private float gunShake = 1.2f;
    [SerializeField] private float killShake = 1.8f;
    [SerializeField] private float parryShake = 1f;
    [SerializeField] private float playerDamageShake = 1f;

    [Header("Player Damage UI")]
    [SerializeField] private RectTransform healthBarPunchTarget;
    [SerializeField] private Image damageScreenFlash;

    [SerializeField, Min(1f)]
    private float healthBarPunchScale = 1.1f;

    [SerializeField, Min(0f)]
    private float healthBarPunchDuration = 0.15f;

    [SerializeField, Range(0f, 1f)]
    private float damageScreenFlashAlpha = 0.35f;

    [SerializeField, Min(0f)]
    private float damageScreenFlashDuration = 0.15f;

    private Coroutine hitStopRoutine;
    private float previousTimeScale;
    private float previousFixedDeltaTime;
    private int lastImpactFrame = -1;

    private Coroutine healthBarPunchRoutine;
    private Coroutine damageScreenFlashRoutine;

    private Vector3 healthBarNormalScale = Vector3.one;
    private bool healthBarScaleCaptured;

    [Header("Cannon Fire UI")]
    [SerializeField] private Image cannonFireFlash;
    [SerializeField] private Color cannonFireFlashColour =
        new Color(1f, 0.7f, 0.25f, 1f);
    [SerializeField, Range(0f, 1f)] private float cannonFireFlashAlpha = 0.65f;
    [SerializeField, Min(0.01f)] private float cannonFireFlashDuration = 0.12f;

    private Coroutine cannonFireFlashRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ResolveDamageUiReferences();

        if (damageScreenFlash != null)
        {
            SetDamageScreenFlashAlpha(0f);
        }
    }

    private void OnEnable()
    {
        StationHitByCannon.ShotsFired += PlayCannonFireFlash;
    }

    public void PlayAttackSound(EnemyHitType hitType)
    {
        switch (hitType)
        {
            case EnemyHitType.Melee:
                PlaySound(meleeSwing);
                break;

            case EnemyHitType.Gun:
                PlaySound(gunshot);
                break;
        }
    }

    public void PlayEnemyImpact(EnemyHitType hitType, bool killed)
    {
        // Avoid stacking impact sounds when one swing hits several colliders.
        if (lastImpactFrame == Time.frameCount && !killed)
            return;

        lastImpactFrame = Time.frameCount;

        if (killed)
        {
            PlaySound(enemyKilled);
            Shake(killShake);
            StartHitStop(killHitStop);
            return;
        }

        switch (hitType)
        {
            case EnemyHitType.Melee:
                PlaySound(meleeImpact);
                Shake(meleeShake);
                StartHitStop(meleeHitStop);
                break;

            case EnemyHitType.Gun:
                PlaySound(gunImpact);
                Shake(gunShake);
                StartHitStop(gunHitStop);
                break;
        }
    }

    public void PlayParry()
    {
        PlaySound(parry);
        Shake(parryShake);
        StartHitStop(parryHitStop);
    }

    private void PlaySound(AudioClip clip)
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(clip);
    }

    private void Shake(float magnitude)
    {
        if (CameraShake.Instance != null)
            CameraShake.Instance.TriggerShake(0.1f, magnitude);
    }

    private void StartHitStop(float duration)
    {
        if (duration <= 0f || Time.timeScale <= 0f)
            return;

        if (hitStopRoutine != null)
        {
            StopCoroutine(hitStopRoutine);
            RestoreTime();
        }

        hitStopRoutine = StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        previousTimeScale = Time.timeScale;
        previousFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = previousTimeScale * hitStopScale;
        Time.fixedDeltaTime = previousFixedDeltaTime * hitStopScale;

        yield return new WaitForSecondsRealtime(duration);

        RestoreTime();
        hitStopRoutine = null;
    }

    private void RestoreTime()
    {
        Time.timeScale = previousTimeScale;
        Time.fixedDeltaTime = previousFixedDeltaTime;
    }

    private void OnDisable()
    {
        StationHitByCannon.ShotsFired -= PlayCannonFireFlash;

        if (cannonFireFlashRoutine != null)
        {
            StopCoroutine(cannonFireFlashRoutine);
            cannonFireFlashRoutine = null;
        }

        SetCannonFireFlashAlpha(0f);

        if (hitStopRoutine != null)
        {
            StopCoroutine(hitStopRoutine);
            RestoreTime();
            hitStopRoutine = null;
        }

        if (healthBarPunchRoutine != null)
        {
            StopCoroutine(healthBarPunchRoutine);
            healthBarPunchRoutine = null;
        }

        if (damageScreenFlashRoutine != null)
        {
            StopCoroutine(damageScreenFlashRoutine);
            damageScreenFlashRoutine = null;
        }

        if (healthBarPunchTarget != null &&
            healthBarScaleCaptured)
        {
            healthBarPunchTarget.localScale =
                healthBarNormalScale;
        }

        SetDamageScreenFlashAlpha(0f);
    }

    public void CancelHitStop()
    {
        if (hitStopRoutine == null)
            return;

        StopCoroutine(hitStopRoutine);
        hitStopRoutine = null;
        RestoreTime();
    }

    public void PlayPlayerDamaged()
    {
        PlaySound(playerHurt);
        Shake(playerDamageShake);
        StartHitStop(playerDamageHitStop);

        PlayHealthBarPunch();
        PlayDamageScreenFlash();
    }

    private void ResolveDamageUiReferences()
    {
        if (healthBarPunchTarget == null)
        {
            GameObject healthBarObject = GameObject.Find("HealthBar");

            if (healthBarObject != null)
            {
                healthBarPunchTarget = healthBarObject.GetComponent<RectTransform>();
            }
        }

        if (healthBarPunchTarget != null && !healthBarScaleCaptured)
        {
            healthBarNormalScale = healthBarPunchTarget.localScale;
            healthBarScaleCaptured = true;
        }

        if (damageScreenFlash == null)
        {
            GameObject flashObject = GameObject.Find("DamageScreenFlash");

            if (flashObject != null)
            {
                damageScreenFlash = flashObject.GetComponent<Image>();
            }
        }
    }

    private void PlayHealthBarPunch()
    {
        ResolveDamageUiReferences();

        if (healthBarPunchTarget == null)
        {
            return;
        }

        if (healthBarPunchRoutine != null)
        {
            StopCoroutine(healthBarPunchRoutine);
            healthBarPunchTarget.localScale = healthBarNormalScale;
        }

        healthBarPunchRoutine = StartCoroutine(HealthBarPunchRoutine());
    }

    private IEnumerator HealthBarPunchRoutine()
    {
        Vector3 punchedScale = healthBarNormalScale * healthBarPunchScale;

        healthBarPunchTarget.localScale = punchedScale;

        if (healthBarPunchDuration <= 0f)
        {
            healthBarPunchTarget.localScale = healthBarNormalScale;
            healthBarPunchRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < healthBarPunchDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / healthBarPunchDuration);

            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            healthBarPunchTarget.localScale = Vector3.Lerp(punchedScale, healthBarNormalScale, easedProgress);

            yield return null;
        }

        healthBarPunchTarget.localScale = healthBarNormalScale;
        healthBarPunchRoutine = null;
    }

    private void PlayDamageScreenFlash()
    {
        ResolveDamageUiReferences();

        if (damageScreenFlash == null)
        {
            return;
        }

        if (damageScreenFlashRoutine != null)
        {
            StopCoroutine(damageScreenFlashRoutine);
        }

        damageScreenFlashRoutine = StartCoroutine(DamageScreenFlashRoutine());
    }

    private IEnumerator DamageScreenFlashRoutine()
    {
        if (damageScreenFlashDuration <= 0f)
        {
            SetDamageScreenFlashAlpha(0f);
            damageScreenFlashRoutine = null;
            yield break;
        }

        SetDamageScreenFlashAlpha(damageScreenFlashAlpha);

        float elapsed = 0f;

        while (elapsed < damageScreenFlashDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / damageScreenFlashDuration);

            float alpha = Mathf.Lerp(damageScreenFlashAlpha, 0f, progress);

            SetDamageScreenFlashAlpha(alpha);

            yield return null;
        }

        SetDamageScreenFlashAlpha(0f);
        damageScreenFlashRoutine = null;
    }

    private void SetDamageScreenFlashAlpha(float alpha)
    {
        if (damageScreenFlash == null)
        {
            return;
        }

        Color color = damageScreenFlash.color;
        color.a = alpha;
        damageScreenFlash.color = color;
    }

    private void PlayCannonFireFlash()
    {
        if (cannonFireFlash == null)
        {
            return;
        }

        if (cannonFireFlashRoutine != null)
        {
            StopCoroutine(cannonFireFlashRoutine);
        }

        cannonFireFlashRoutine = StartCoroutine(CannonFireFlashRoutine());
    }

    private IEnumerator CannonFireFlashRoutine()
    {
        cannonFireFlash.color = new Color(cannonFireFlashColour.r, cannonFireFlashColour.g, cannonFireFlashColour.b, cannonFireFlashAlpha
        );

        float elapsed = 0f;

        while (elapsed < cannonFireFlashDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / cannonFireFlashDuration);

            SetCannonFireFlashAlpha(Mathf.Lerp(cannonFireFlashAlpha, 0f, progress));

            yield return null;
        }

        SetCannonFireFlashAlpha(0f);
        cannonFireFlashRoutine = null;
    }

    private void SetCannonFireFlashAlpha(float alpha)
    {
        if (cannonFireFlash == null)
        {
            return;
        }

        Color colour = cannonFireFlash.color;
        colour.a = alpha;
        cannonFireFlash.color = colour;
    }
}