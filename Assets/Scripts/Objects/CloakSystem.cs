using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class CloakSystem : InteractableObject
{
    [Header("Cloak Settings")]
    [SerializeField] private float cloakDuration = 5f;
    [SerializeField] private float cooldownDuration = 10f;

    public static event Action<float> OnCloakActivated;

    private bool isOnCooldown;
    public override void Interact()
    {
        if(RunManager.Instance.isCloaked || isOnCooldown)
        {
            Debug.Log("Cloak could not be activated due to being active or on cooldown. Wait till recharged");
            return;
        }  

        OnCloakActivated?.Invoke(cloakDuration);
        StartCoroutine(CloakTimerSequence());
    }

    private IEnumerator CloakTimerSequence()
    {
        RunManager.Instance.isCloaked = true;

        Debug.Log("Cloak activated");

        yield return new WaitForSeconds(cloakDuration);

        RunManager.Instance.isCloaked = false;
        isOnCooldown = true;
        Debug.Log("Cloak Wore Off. Cooldown Started.");

        yield return new WaitForSeconds(cooldownDuration);

        isOnCooldown = false;
        Debug.Log("Cloak Ready to Use Again!");
    }
}
