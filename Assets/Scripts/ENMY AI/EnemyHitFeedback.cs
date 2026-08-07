using System.Collections;
using UnityEngine;

public class EnemyHitFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private DamageNumber damageNumberPrefab;
    [SerializeField] private Transform damageNumberPoint;

    [Header("Flash")]
    [SerializeField] private Color meleeFlashColor = Color.white;
    [SerializeField] private Color gunFlashColor =
        new Color(1f, 0.35f, 0.2f);
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Animation")]
    [SerializeField] private bool useHurtAnimation;
    [SerializeField] private string hurtTriggerName = "hurt";

    private Color originalColor;
    private Coroutine flashRoutine;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void PlayHit(int damage, EnemyHitType hitType, bool killed)
    {
        if (hitType != EnemyHitType.Environment)
        {
            Color flashColor = hitType == EnemyHitType.Gun
                ? gunFlashColor
                : meleeFlashColor;

            if (flashRoutine != null)
                StopCoroutine(flashRoutine);

            flashRoutine = StartCoroutine(FlashRoutine(flashColor));

            CombatFeedback.Instance?.PlayEnemyImpact(hitType, killed);
        }

        SpawnDamageNumber(damage, killed);

        if (!killed && useHurtAnimation && animator != null)
            animator.SetTrigger(hurtTriggerName);
    }

    private IEnumerator FlashRoutine(Color flashColor)
    {
        spriteRenderer.color = flashColor;

        // Continue counting while the game is in hit-stop.
        yield return new WaitForSecondsRealtime(flashDuration);

        spriteRenderer.color = originalColor;
        flashRoutine = null;
    }

    private void SpawnDamageNumber(int damage, bool killed)
    {
        if (damageNumberPrefab == null)
            return;

        Vector3 position = damageNumberPoint != null
            ? damageNumberPoint.position
            : transform.position + Vector3.up * 30f;

        DamageNumber number = Instantiate(
            damageNumberPrefab,
            position,
            Quaternion.identity);

        number.Initialize(damage, killed);
    }
}