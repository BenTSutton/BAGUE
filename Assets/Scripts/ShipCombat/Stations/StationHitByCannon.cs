using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class StationHitByCannon : MonoBehaviour
{
    [SerializeField, Min(0f)] private float firingAnticipation = 0.18f;
    [SerializeField, Min(0f)] private float projectileTravelTime = 0.3f;
    [SerializeField, Min(0f)] private float impactObservationTime = 0.45f;
    [SerializeField] private UnityEvent onTargetLocked;
    [SerializeField] private UnityEvent onImpact;

    public static event Action<EnemyShipStation> TargetLocked;
    public static event Action ShotsFired;
    public static event Action<EnemyShipStation> ShotImpacted;
    public static event Action ShotResolutionFinished;

    public static bool IsResolvingShot => activeResolver != null;

    private static StationHitByCannon activeResolver;
    private EnemyShipStation lockedTarget;
    private float lockedDamage;
    private Coroutine shotRoutine;
    public void DamageEnemyStation(EnemyShipStation station)
    {
        if (activeResolver != null)
        {
            return;
        }

        if (GameManager.Instance == null ||
            GameManager.Instance.currentState != GameState.Aiming ||
            GameManager.Instance.IsCombatEnding)
        {
            return;
        }

        if (RunManager.Instance == null ||
            station == null ||
            !station.CanReceiveCannonShot)
        {
            return;
        }

        Cannon cannon = RunManager.Instance.activeCannon;

        if (cannon == null)
        {
            Debug.LogWarning("[StationHitByCannon] No active cannon.", this);

            return;
        }

        float damage = cannon.strength;

        if (!cannon.TryConsumeLoadedShot())
        {
            return;
        }

        activeResolver = this;
        lockedTarget = station;
        lockedDamage = damage;

        RunManager.Instance.RecordCannonShot();

        onTargetLocked?.Invoke();
        TargetLocked?.Invoke(lockedTarget);

        shotRoutine = StartCoroutine(ResolveShot());
    }

    private IEnumerator ResolveShot()
    {
        if(firingAnticipation > 0f) 
        {
            yield return new WaitForSecondsRealtime(firingAnticipation);
        }
        if(!CanContinue()) 
        { 
            CompleteResolution(); 
            yield break; 
        }

        ShotsFired?.Invoke();

        if(projectileTravelTime > 0f) 
        {
            yield return new WaitForSecondsRealtime(projectileTravelTime);
        }
        if(!CanContinue()) 
        { 
            CompleteResolution(); 
            yield break; 
        }

        EnemyShipStation impactedTarget = lockedTarget;

        impactedTarget.DamageShipStation(lockedDamage);

        onImpact?.Invoke();
        ShotImpacted?.Invoke(impactedTarget);

        if (impactObservationTime > 0f)
        {
            yield return new WaitForSecondsRealtime(impactObservationTime);
        }

        CompleteResolution();
    }

    private bool CanContinue()
    {
        return activeResolver == this && lockedTarget != null && lockedTarget.CanReceiveCannonShot && GameManager.Instance != null && !GameManager.Instance.IsCombatEnding;
    }

    private void CompleteResolution()
    {
        if(activeResolver != this) 
        {
            return;
        }

        activeResolver = null;
        lockedTarget = null;
        shotRoutine = null;
        ShotResolutionFinished?.Invoke();
    }

    private void OnDisable()
    {
        if(activeResolver != this) 
        {
            return;
        }

        if(shotRoutine != null) 
        {
            StopCoroutine(shotRoutine);
        }

        CompleteResolution();
    }
}
