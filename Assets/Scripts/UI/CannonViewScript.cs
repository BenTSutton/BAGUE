using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class CannonViewScript : MonoBehaviour
{
    [SerializeField] private RectTransform crosshair;
    [SerializeField] private Canvas thisCanvas;

    [Header("Shot Feedback")]
    [SerializeField] private AudioClip firingSound;
    [SerializeField] private AudioClip impactSound;
    [SerializeField, Min(0f)] private float firingShakeDuration = 0.12f;
    [SerializeField, Min(0f)] private float firingShakeMagnitude = 4f;
    [SerializeField, Min(0f)] private float impactShakeDuration = 0.18f;
    [SerializeField, Min(0f)] private float impactShakeMagnitude = 8f;
    [SerializeField] private UnityEvent onTargetLocked;
    [SerializeField] private UnityEvent onMuzzleFlash;
    [SerializeField] private UnityEvent onRecoil;
    [SerializeField] private UnityEvent onImpact;
    [Header("Target Confirmation")]
    [SerializeField, Min(0f)] private float targetConfirmationDuration = 0.06f;

    private Coroutine leaveAimingRoutine;
    void OnEnable()
    {
        //Cursor.visible = false;
        StationHitByCannon.TargetLocked += HandleTargetLocked;
        StationHitByCannon.ShotsFired += HandleShotFired;
        StationHitByCannon.ShotImpacted += HandleShotImpacted;
        StationHitByCannon.ShotResolutionFinished += HandleShotFinished;

        if(crosshair != null) 
        {
            crosshair.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        StationHitByCannon.TargetLocked -= HandleTargetLocked;
        StationHitByCannon.ShotsFired -= HandleShotFired;
        StationHitByCannon.ShotImpacted -= HandleShotImpacted;
        StationHitByCannon.ShotResolutionFinished -= HandleShotFinished;

        if (leaveAimingRoutine != null)
        {
            StopCoroutine(leaveAimingRoutine);
            leaveAimingRoutine = null;
        }
    }

    private void Update()
    {
        if (thisCanvas == null || !thisCanvas.enabled)
        {
            return;
        }

        if (StationHitByCannon.IsResolvingShot)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelAiming();
            return;
        }

        crosshair.position = Input.mousePosition;
    }

    private void CancelAiming()
    {
        Cannon activeCannon = RunManager.Instance?.activeCannon;

        if (activeCannon != null)
        {
            activeCannon.CancelAiming();
        }
        else
        {
            if (thisCanvas != null)
            {
                thisCanvas.enabled = false;
            }

            if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.Aiming)
            {
                GameManager.Instance.ChangeState(GameState.Combat);
            }
        }

        Cursor.visible = true;
    }

    private void HandleTargetLocked(EnemyShipStation target)
    {
        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(false);
        }

        onTargetLocked?.Invoke();

        if (leaveAimingRoutine != null)
        {
            StopCoroutine(leaveAimingRoutine);
        }

        leaveAimingRoutine = StartCoroutine(LeaveAimingAfterTargetLock());
    }

    private IEnumerator LeaveAimingAfterTargetLock()
    {
        if (targetConfirmationDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(targetConfirmationDuration);
        }

        if (thisCanvas != null)
        {
            thisCanvas.enabled = false;
        }

        if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.Aiming)
        {
            GameManager.Instance.ChangeState(GameState.Combat);
        }

        Cursor.visible = true;
        leaveAimingRoutine = null;
    }

    private void HandleShotFired()
    {
        onMuzzleFlash?.Invoke();
        onRecoil?.Invoke();
        SFXManager.Instance?.PlaySFX(firingSound);
        TriggerRealTimeShake(firingShakeDuration, firingShakeMagnitude);
    }

    private void HandleShotImpacted(EnemyShipStation target)
    {
        onImpact?.Invoke();
        SFXManager.Instance?.PlaySFX(impactSound);
        TriggerRealTimeShake(impactShakeDuration, impactShakeMagnitude);
    }

    private void HandleShotFinished()
    {
        crosshair.gameObject.SetActive(true);

        Cannon activeCannon = RunManager.Instance?.activeCannon;

        if (activeCannon != null)
        {
            activeCannon.CloseAiming();
        }
        else if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.Aiming)
        {
            GameManager.Instance.ChangeState(GameState.Combat);
        }

        if (thisCanvas != null)
        {
            thisCanvas.enabled = false;
        }

        Cursor.visible = true;
    }

    private void TriggerRealTimeShake(float duration, float magnitude)
    {
        if(CameraShake.Instance == null) 
        {
            return;
        }
        float adjustedDuration = duration * Mathf.Max(Time.timeScale, 0.01f);
        CameraShake.Instance.TriggerShake(adjustedDuration, magnitude);
    }

    public void ShowAiming()
    {
        if (leaveAimingRoutine != null)
        {
            StopCoroutine(leaveAimingRoutine);
            leaveAimingRoutine = null;
        }

        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(true);
        }

        if (thisCanvas != null)
        {
            thisCanvas.enabled = true;
        }
    }
}
