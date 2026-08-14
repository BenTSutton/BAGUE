using System;
using System.Collections;
using UnityEngine;

public class CloakSystem : InteractableObject
{
    [Header("Cloak Settings")]
    [SerializeField] private float cloakDuration = 5f;
    [SerializeField] private float cooldownDuration = 10f;

    public static event Action<float> OnCloakActivated;
    private Coroutine cloakRoutine;
    private bool isOnCooldown;
    public bool IsOnCooldown => isOnCooldown;
    public override void Interact()
    {
        if (RunManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsCombatEnding)
        {
            return;
        }

        if (RunManager.Instance.isCloaked || isOnCooldown)
        {
            Debug.Log("[CloakSystem] Cloak is active or cooling down.", this);

            return;
        }

        OnCloakActivated?.Invoke(cloakDuration);
        SFXManager.Instance?.PlayUseCloak();
        cloakRoutine = StartCoroutine(CloakTimerSequence());
    }

    private IEnumerator CloakTimerSequence()
    {
        RunManager.Instance.isCloaked = true;

        Debug.Log("Cloak activated");

        yield return new WaitForSeconds(cloakDuration);

        RunManager.Instance.isCloaked = false;
        isOnCooldown = true;
        Debug.Log("Cloak Wore Off. Cooldown Started.");

        float cooldownMultiplier;

        if (RunManager.Instance.IsRoomOperational<EngineRoom>())
        {
            EngineRoom engineRoom =
                RunManager.Instance.GetRoomData<EngineRoom>();

            int engineLevel =
                RunManager.Instance.GetRoomLevel<EngineRoom>();

            cooldownMultiplier =
                engineRoom.GetCloakCooldownMultiplier(engineLevel);
        }
        else
        {
            cooldownMultiplier = 1.75f;
        }

        float finalCooldown = cooldownDuration * cooldownMultiplier;

        yield return new WaitForSeconds(finalCooldown);

        isOnCooldown = false;
        Debug.Log("Cloak Ready to Use Again!");
    }

    private void OnDisable()
    {
        if (cloakRoutine != null)
        {
            StopCoroutine(cloakRoutine);
            cloakRoutine = null;
        }

        if (RunManager.Instance != null)
        {
            RunManager.Instance.isCloaked = false;
        }

        isOnCooldown = false;
    }
}
