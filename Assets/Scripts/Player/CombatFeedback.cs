using System.Collections;
using UnityEngine;

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

    [Header("Hit-stop")]
    [SerializeField, Range(0f, 1f)] private float hitStopScale = 0.05f;
    [SerializeField] private float meleeHitStop = 0.045f;
    [SerializeField] private float gunHitStop = 0.065f;
    [SerializeField] private float killHitStop = 0.08f;
    [SerializeField] private float parryHitStop = 0.07f;

    [Header("Camera shake")]
    [SerializeField] private float meleeShake = 0.5f;
    [SerializeField] private float gunShake = 1.2f;
    [SerializeField] private float killShake = 1.8f;
    [SerializeField] private float parryShake = 1f;

    private Coroutine hitStopRoutine;
    private float previousTimeScale;
    private float previousFixedDeltaTime;
    private int lastImpactFrame = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
        if (hitStopRoutine != null)
        {
            StopCoroutine(hitStopRoutine);
            RestoreTime();
            hitStopRoutine = null;
        }
    }
}