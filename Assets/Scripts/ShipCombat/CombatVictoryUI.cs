using System.Collections;
using TMPro;
using UnityEngine;

public class CombatVictoryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private CanvasGroup titleGroup;
    [SerializeField] private CanvasGroup rewardsGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text rewardsText;

    [Header("Presentation")]
    [SerializeField, Min(0f)] private float titleRevealDuration = 0.25f;
    [SerializeField, Min(0f)] private float rewardsRevealDuration = 0.3f;
    [SerializeField, Min(0f)] private float rewardsDelayAfterFinalExplosion = 0.15f;
    [SerializeField, Range(0f, 0.5f)] private float titlePunchAmount = 0.15f;
    [SerializeField, Range(0f, 0.5f)] private float rewardsPunchAmount = 0.08f;

    private Coroutine titleRoutine;
    private Coroutine rewardsRoutine;

    private Vector3 titleNormalScale = Vector3.one;
    private Vector3 rewardsNormalScale = Vector3.one;

    private void Awake()
    {
        if (titleGroup != null)
        {
            titleNormalScale = titleGroup.transform.localScale;
            SetGroupHidden(titleGroup);
        }

        if (rewardsGroup != null)
        {
            rewardsNormalScale = rewardsGroup.transform.localScale;
            SetGroupHidden(rewardsGroup);
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        EnemyShip.OnEnemyShipDefeated += ShowVictoryTitle;
        EnemyShip.OnEnemyShipFinalExplosion += HandleFinalExplosion;
    }

    private void OnDisable()
    {
        EnemyShip.OnEnemyShipDefeated -= ShowVictoryTitle;
        EnemyShip.OnEnemyShipFinalExplosion -= HandleFinalExplosion;

        if (titleRoutine != null)
        {
            StopCoroutine(titleRoutine);
            titleRoutine = null;
        }

        if (rewardsRoutine != null)
        {
            StopCoroutine(rewardsRoutine);
            rewardsRoutine = null;
        }
    }

    private void ShowVictoryTitle(EnemyShip defeatedShip)
    {
        if (defeatedShip == null)
        {
            return;
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = "VICTORY";
        }

        PrepareRewardsText();
        SetGroupHidden(rewardsGroup);

        if (titleRoutine != null)
        {
            StopCoroutine(titleRoutine);
        }

        titleRoutine = StartCoroutine(RevealTitleRoutine());
    }

    private void HandleFinalExplosion(EnemyShip defeatedShip)
    {
        if (defeatedShip == null || victoryPanel == null)
        {
            return;
        }

        if (rewardsRoutine != null)
        {
            StopCoroutine(rewardsRoutine);
        }

        rewardsRoutine = StartCoroutine(RevealRewardsAfterFinalExplosion());
    }

    private void PrepareRewardsText()
    {
        if (rewardsText == null)
        {
            return;
        }

        CombatDefinition definition = GameManager.Instance != null
            ? GameManager.Instance.currentCombatNode
            : null;

        if (definition == null)
        {
            rewardsText.text = "Salvage calculation unavailable";
            return;
        }

        rewardsText.text =
            $"Credits: {definition.rewardCredits}\n" +
            $"Scrap: {definition.rewardScrap}\n" +
            $"Fuel: {definition.rewardFuel}";
    }

    private IEnumerator RevealTitleRoutine()
    {
        yield return RevealGroup(titleGroup, titleNormalScale, titleRevealDuration, titlePunchAmount);

        titleRoutine = null;
    }

    private IEnumerator RevealRewardsAfterFinalExplosion()
    {
        if (rewardsDelayAfterFinalExplosion > 0f)
        {
            yield return new WaitForSecondsRealtime(rewardsDelayAfterFinalExplosion);
        }

        yield return RevealGroup(rewardsGroup, rewardsNormalScale, rewardsRevealDuration, rewardsPunchAmount);

        rewardsRoutine = null;
    }

    private IEnumerator RevealGroup(CanvasGroup group, Vector3 normalScale, float duration, float punchAmount)
    {
        if (group == null)
        {
            yield break;
        }

        group.gameObject.SetActive(true);
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        RectTransform rectTransform = group.transform as RectTransform;

        if (duration <= 0f)
        {
            group.alpha = 1f;

            if (rectTransform != null)
            {
                rectTransform.localScale = normalScale;
            }

            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            float punch = Mathf.Sin(progress * Mathf.PI) * punchAmount;

            float scale = Mathf.Lerp(0.85f, 1f, easedProgress) + punch;

            group.alpha = easedProgress;

            if (rectTransform != null)
            {
                rectTransform.localScale = normalScale * scale;
            }

            yield return null;
        }

        group.alpha = 1f;

        if (rectTransform != null)
        {
            rectTransform.localScale = normalScale;
        }
    }

    private void SetGroupHidden(CanvasGroup group)
    {
        if (group == null)
        {
            return;
        }

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        group.gameObject.SetActive(false);
    }
}