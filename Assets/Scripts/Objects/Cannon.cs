using UnityEngine;
using System;

public class Cannon : InteractableObject
{
    [SerializeField] private float cannonDamage;
    public Canvas cannonView;
     [Header("Reload")]
    [SerializeField, Min(0.1f)]
    private float reloadDuration = 6f;

    [SerializeField]
    private bool startLoaded = true;

    private bool isLoaded;
    private bool reloadPaused;
    private float reloadElapsed;
    private float reloadRateMultiplier = 1f;

    public event Action<float> ReloadProgressChanged;
    public event Action CannonReady;
    public event Action<bool> ReloadPauseChanged;

    public bool IsLoaded => isLoaded;
    public bool IsReloadPaused => reloadPaused;
    public float ReloadRateMultiplier => reloadRateMultiplier;

    public float strength
    {
        get
        {
            WeaponsRoom weaponsRoom = RunManager.Instance.GetRoomData<WeaponsRoom>();
            int weaponsLevel = RunManager.Instance.GetRoomLevel<WeaponsRoom>();

            float damage = weaponsRoom.ModifyCannonDamage(cannonDamage, weaponsLevel);
            return RunManager.Instance.ModifyCannonDamage(damage);
        }
    }

    public float ReloadProgress
    {
        get
        {
            if (isLoaded)
                return 1f;

            if (reloadDuration <= 0f)
                return 1f;

            return Mathf.Clamp01(reloadElapsed / reloadDuration);
        }
    }

    private void Start()
    {
        isLoaded = startLoaded;
        reloadElapsed = isLoaded ? reloadDuration : 0f;

        ReloadProgressChanged?.Invoke(ReloadProgress);

        if (isLoaded)
        {
            CannonReady?.Invoke();
        }
    }

    private void Update()
    {
        if (isLoaded || reloadPaused)
            return;

        if (reloadDuration <= 0f)
        {
            CompleteReload();
            return;
        }

        reloadElapsed = Mathf.Min(
            reloadElapsed + Time.deltaTime * reloadRateMultiplier,
            reloadDuration);

        ReloadProgressChanged?.Invoke(ReloadProgress);

        if (reloadElapsed >= reloadDuration)
        {
            CompleteReload();
        }
    }

    public override void Interact()
    {
        if (GameManager.Instance == null || RunManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.IsCombatEnding || StationHitByCannon.IsResolvingShot)
        {
            return;
        }

        if (GameManager.Instance.currentState == GameState.Combat)
        {
            if (!isLoaded)
            {
                Debug.Log($"[Cannon] Reloading: {ReloadProgress:P0}", this);

                return;
            }

            OpenAiming();
            return;
        }

        if (GameManager.Instance.currentState == GameState.Aiming && RunManager.Instance.activeCannon == this)
        {
            CloseAiming();
        }
    }

    private void OpenAiming()
    {
        if (!isLoaded)
        {
            return;
        }

        if (GameManager.Instance == null || RunManager.Instance == null || cannonView == null)
        {
            return;
        }

        if (GameManager.Instance.IsCombatEnding)
        {
            return;
        }

        EnemyShip enemyShip = RunManager.Instance.activeEnemyShip;

        if (enemyShip == null || enemyShip.IsDefeated)
        {
            return;
        }

        RunManager.Instance.activeCannon = this;

        CannonViewScript cannonViewScript = cannonView.GetComponent<CannonViewScript>();

        if (cannonViewScript != null)
        {
            cannonViewScript.ShowAiming();
        }
        else
        {
            cannonView.enabled = true;
        }

        GameManager.Instance.ChangeState(GameState.Aiming);
        SFXManager.Instance?.PlayOpenCannon();
    }

    public void CloseAiming()
    {
        if (cannonView != null)
        {
            cannonView.enabled = false;
        }

        if (RunManager.Instance != null && RunManager.Instance.activeCannon == this)
        {
            RunManager.Instance.activeCannon = null;
        }

        if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.Aiming)
        {
            GameManager.Instance.ChangeState(GameState.Combat);
        }

        Cursor.visible = true;
    }

    public void CancelAiming()
    {
        CloseAiming();
    }

    public bool TryConsumeLoadedShot()
    {
        if (!isLoaded)
        {
            return false;
        }

        if (RunManager.Instance == null || RunManager.Instance.activeCannon != this)
        {
            return false;
        }

        if (GameManager.Instance == null || GameManager.Instance.IsCombatEnding)
        {
            return false;
        }

        isLoaded = false;
        reloadElapsed = 0f;

        SFXManager.Instance?.PlayPlayerCannonReload();
        ReloadProgressChanged?.Invoke(0f);

        return true;
    }

    public void SetReloadPaused(bool paused)
    {
        if (reloadPaused == paused)
        {
            return;
        }

        reloadPaused = paused;
        ReloadPauseChanged?.Invoke(reloadPaused);
    }

    public void SetReloadRateMultiplier(float multiplier)
    {
        reloadRateMultiplier = Mathf.Max(0.01f, multiplier);
    }

    private void CompleteReload()
    {
        if (isLoaded)
            return;

        reloadElapsed = reloadDuration;
        isLoaded = true;

        ReloadProgressChanged?.Invoke(1f);
        CannonReady?.Invoke();
    }
}
