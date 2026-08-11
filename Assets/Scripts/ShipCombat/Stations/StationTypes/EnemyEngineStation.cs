using System;
using UnityEngine;

public class EnemyEngineStation :
    EnemyShipStation,
    IChargeableStation
{
    [Header("Escape")]
    [SerializeField, Min(1f)]
    private float escapeDuration = 75f;

    private float escapeElapsed;
    private bool escapeReported;

    // Reuses existing charge-bar and mirrored station UI.
    public event Action<float, float> OnChargeChanged;

    // Remaining time, total duration.
    public static event Action<float, float> EscapeCountdownChanged;

    public static event Action EscapePrevented;
    public static event Action EnemyEscaped;

    public float RemainingEscapeTime => Mathf.Max(0f, escapeDuration - escapeElapsed);

    public override string BrokenMessage =>
        "ENGINE DESTROYED — ENEMY ESCAPE PREVENTED";

    private void Start()
    {
        PublishCountdown();
    }

    private void Update()
    {
        if (stationIsBroken || escapeReported || !CombatIsRunning())
        {
            return;
        }

        escapeElapsed = Mathf.Min(escapeElapsed + Time.deltaTime, escapeDuration);

        PublishCountdown();

        if (escapeElapsed < escapeDuration)
            return;

        escapeReported = true;

        Debug.Log("[EnemyEngineStation] Enemy escaped.", this);

        EnemyEscaped?.Invoke();
    }

    private bool CombatIsRunning()
    {
        if(GameManager.Instance == null || GameManager.Instance.IsCombatEnding)
        { 
            return false;
        }
        if(enemyShip == null || enemyShip.IsDefeated) 
        {
            return false;
        }

        GameState state = GameManager.Instance.currentState;
        return state == GameState.Combat || state == GameState.Aiming;
    }

    private void PublishCountdown()
    {
        OnChargeChanged?.Invoke(escapeElapsed, escapeDuration);

        EscapeCountdownChanged?.Invoke(RemainingEscapeTime, escapeDuration);
    }

    public override void HandleBrokenStation()
    {
        escapeReported = true;

        Debug.Log("[EnemyEngineStation] Escape countdown stopped.", this);

        PublishCountdown();
        EscapePrevented?.Invoke();
    }
}