using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyShieldStation : EnemyShipStation
{
    [SerializeField] private GameObject shieldVisuals;
    [SerializeField] private ShieldColourGradient shieldGradient;
    private Image shieldImage;
    private float shieldAlpha = 0.3f;

    protected override void Awake()
    {
        // Finds the ship component on this object or any parent
        thisShip = GetComponentInParent<EnemyShip>();
        if (shieldVisuals != null)
        {
            shieldImage = shieldVisuals.GetComponent<Image>();
        }

        if (thisShip != null)
        {
            thisShip.EnableShield();
        }
    }

    private void Start()
    {
        if (thisShip != null && shieldVisuals != null)
        {
            shieldImage.enabled = true;
            
            UpdateShieldColour(thisShip.GetShieldHealth, thisShip.GetShieldMaxHealth);
        }
    }

    private void UpdateShieldColour(float shieldHealth, float shieldMaxHealth)
    {
        if (shieldImage == null || shieldGradient == null) return;
        
        Color calculatedColor = shieldGradient.GetColor(shieldHealth, shieldMaxHealth);
        calculatedColor.a = shieldAlpha;
        shieldImage.color = calculatedColor;;
    }

    private void OnEnable()
    {
        thisShip.OnEnemyShieldBreak += HandleBrokenStation;
        EnemyShip.OnEnemyShieldDamaged += UpdateShieldColour;
        EnemyShip.OnEnemyShieldRepaired += EnableShield;
    }

    private void OnDisable()
    {
        thisShip.OnEnemyShieldBreak -= HandleBrokenStation;
        EnemyShip.OnEnemyShieldDamaged -= UpdateShieldColour;
        EnemyShip.OnEnemyShieldRepaired -= EnableShield;     
    }

    public override void HandleBrokenStation()
    {
        DisableShield();
    }

    private void DisableShield()
    {
        shieldImage.enabled = false;
    }

    private void EnableShield(float shieldHealth, float shieldMaxHealth)
    {
        shieldImage.enabled = true;
        UpdateShieldColour(shieldHealth, shieldMaxHealth);
    }
}
