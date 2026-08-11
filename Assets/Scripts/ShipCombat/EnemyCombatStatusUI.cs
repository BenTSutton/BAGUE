using System.Collections;
using TMPro;
using UnityEngine;

public class EnemyCombatStatusUI : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text escapeCountdownText;

    [SerializeField, Min(0.1f)]
    private float messageDuration = 3f;

    private Coroutine messageRoutine;

    [Header("Boarding")]
    [SerializeField] private BoardingController boardingController;
    [SerializeField] private GameObject boardingWarningPanel;
    [SerializeField] private TMP_Text boardingWarningText;

    [Header("Incoming Enemy Fire")]
    [SerializeField] private GameObject incomingFirePanel;
    [SerializeField] private TMP_Text incomingFireText;

    [SerializeField] private AudioClip incomingFireSound;
    [SerializeField] private AudioClip cloakEvadeSound;
    [SerializeField] private AudioClip passiveDodgeSound;
    [SerializeField] private AudioClip enemyAttackHitSound;

    [Header("Enemy Desperation")]
    [SerializeField] private GameObject escapeUrgencyIndicator;
    [SerializeField] private AudioClip desperationAlarmSound;

    [SerializeField] private Color desperationEscapeColor =
        new Color(1f, 0.2f, 0.1f, 1f);

    private Color normalEscapeColor = Color.white;
    private bool escapeUrgencyActive;

    private void Awake()
    {
        normalEscapeColor = escapeCountdownText.color;
        escapeUrgencyIndicator.SetActive(false);
    }

    private void OnEnable()
    {
        EnemyShipStationUI.StationMessageRaised += ShowMessage;

        EnemyEngineStation.EscapeCountdownChanged += UpdateEscapeCountdown;

        EnemyEngineStation.EscapePrevented += ShowEscapePrevented;

        EnemyEngineStation.EnemyEscaped += ShowEnemyEscaped;

        EnemyCombatStation.IncomingAttackWarningStarted += HandleIncomingAttackWarning;

        EnemyCombatStation.IncomingAttackWarningEnded += HideIncomingAttackWarning;

        EnemyCombatStation.CloakEvadeSucceeded += HandleCloakEvade;

        EnemyCombatStation.PassiveDodgeSucceeded += HandlePassiveDodge;

        EnemyCombatStation.EnemyAttackHit += HandleEnemyAttackHit;

        CombatManager.EnemyDesperationChanged += HandleEnemyDesperationChanged;

        if (boardingController != null)
        {
            boardingController.WaveWarningStarted += HandleWaveWarningStarted;
            boardingController.WaveArrived += HandleWaveArrived;
            boardingController.WavesStopped += HandleWavesStopped;
        }
    }

    private void OnDisable()
    {
        EnemyShipStationUI.StationMessageRaised -= ShowMessage;

        EnemyEngineStation.EscapeCountdownChanged -= UpdateEscapeCountdown;

        EnemyEngineStation.EscapePrevented -= ShowEscapePrevented;

        EnemyEngineStation.EnemyEscaped -= ShowEnemyEscaped;

        EnemyCombatStation.IncomingAttackWarningStarted -= HandleIncomingAttackWarning;

        EnemyCombatStation.IncomingAttackWarningEnded -= HideIncomingAttackWarning;

        EnemyCombatStation.CloakEvadeSucceeded -= HandleCloakEvade;

        EnemyCombatStation.PassiveDodgeSucceeded -= HandlePassiveDodge;

        EnemyCombatStation.EnemyAttackHit -= HandleEnemyAttackHit;

        CombatManager.EnemyDesperationChanged -= HandleEnemyDesperationChanged;

        SetEscapeUrgency(false);

        HideIncomingAttackWarning();

        if (boardingController != null)
        {
            boardingController.WaveWarningStarted -= HandleWaveWarningStarted;
            boardingController.WaveArrived -= HandleWaveArrived;
            boardingController.WavesStopped -= HandleWavesStopped;
        }

        if (boardingWarningPanel != null)
        {
            boardingWarningPanel.SetActive(false);
        }
    }

    private void UpdateEscapeCountdown(float remainingTime, float totalDuration)
    {
        string label = escapeUrgencyActive
        ? "OVERLOAD ESCAPE"
        : "ENEMY ESCAPE";
        escapeCountdownText.text = $"{label}: {Mathf.CeilToInt(remainingTime)}";
    }

    private void ShowEscapePrevented()
    {
        SetEscapeUrgency(false);
        escapeCountdownText.text ="ESCAPE PREVENTED";
    }

    private void ShowEnemyEscaped()
    {
        if (escapeCountdownText != null)
        {
            escapeCountdownText.text = "ENEMY ESCAPED";
        }

        ShowMessage("ENEMY ESCAPED — NO SALVAGE RECOVERED");
    }

    private void ShowMessage(string message)
    {
        if (messageText == null)
            return;

        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
        }

        messageRoutine = StartCoroutine(ShowMessageRoutine(message));
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = message;

        yield return new WaitForSeconds(messageDuration);

        messageText.gameObject.SetActive(false);
        messageRoutine = null;
    }

    private void HandleWaveWarningStarted(float duration)
{
    if (boardingWarningPanel != null)
    {
        boardingWarningPanel.SetActive(true);
    }

    if (boardingWarningText != null)
    {
        boardingWarningText.text = $"BOARDERS INBOUND - {duration:0} SECONDS";
    }
}

    private void HandleWaveArrived(int enemyCount)
    {
        if (boardingWarningPanel != null)
        {
            boardingWarningPanel.SetActive(false);
        }

        string boarderWord = enemyCount == 1 ? "BOARDER" : "BOARDERS";
        ShowMessage($"{enemyCount} {boarderWord} ABOARD");
    }

    private void HandleWavesStopped()
    {
        if (boardingWarningPanel != null)
        {
            boardingWarningPanel.SetActive(false);
        }
    }

    private void HandleIncomingAttackWarning(float secondsRemaining)
    {
        if (incomingFirePanel != null)
        {
            incomingFirePanel.SetActive(true);
        }

        if (incomingFireText != null)
        {
            incomingFireText.text = $"INCOMING FIRE - CLOAK NOW\n" + $"{Mathf.CeilToInt(secondsRemaining)} SECONDS";
        }

        PlayStatusSound(incomingFireSound);
    }

    private void HideIncomingAttackWarning()
    {
        if (incomingFirePanel != null)
        {
            incomingFirePanel.SetActive(false);
        }
    }

    private void HandleCloakEvade()
    {
        HideIncomingAttackWarning();

        ShowMessage("CLOAK EVADE - ENEMY SHOT MISSED");
        PlayStatusSound(cloakEvadeSound);
    }

    private void HandlePassiveDodge()
    {
        HideIncomingAttackWarning();

        ShowMessage("EVASIVE MANEUVER - SHOT DODGED");
        PlayStatusSound(passiveDodgeSound);
    }

    private void HandleEnemyAttackHit(int damage)
    {
        HideIncomingAttackWarning();

        ShowMessage($"ENEMY SHOT CONNECTED - {damage} DAMAGE");
        PlayStatusSound(enemyAttackHitSound);
    }

    private void PlayStatusSound(AudioClip clip)
    {
        if (clip != null && SFXManager.Instance != null)
        {
            SFXManager.Instance.PlaySFX(clip);
        }
    }

    private void HandleEnemyDesperationChanged(bool active, bool engineOperational)
    {
        if (!active)
        {
            SetEscapeUrgency(false);
            return;
        }

        ShowMessage("Enemy systems overloading!");
        PlayStatusSound(desperationAlarmSound);

        SetEscapeUrgency(engineOperational);
    }

    private void SetEscapeUrgency(bool urgent)
    {
        escapeUrgencyActive = urgent;

        escapeUrgencyIndicator.SetActive(urgent);

        escapeCountdownText.color = urgent ? desperationEscapeColor : normalEscapeColor;
    }
}